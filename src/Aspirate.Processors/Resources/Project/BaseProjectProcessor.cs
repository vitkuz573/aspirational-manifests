namespace Aspirate.Processors.Resources.Project;

/// <summary>
/// A base project component shared between versions 0 and 1 of Aspire.
/// </summary>
public abstract class BaseProjectProcessor(
    IFileSystem fileSystem,
    IAnsiConsole console,
    ISecretProvider secretProvider,
    IContainerCompositionService containerCompositionService,
    IContainerDetailsService containerDetailsService,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    private readonly IReadOnlyCollection<string> _manifests =
    [
        $"{TemplateLiterals.DeploymentType}.yaml",
        $"{TemplateLiterals.ServiceType}.yaml",
    ];

    private readonly Dictionary<string, MsBuildContainerProperties> _containerDetailsCache = [];

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        var project = JsonSerializer.Deserialize<ProjectResource>(ref reader);
        ValidateProjectResource(project);
        return project;
    }

    protected static void ValidateProjectResource(ProjectResource? project)
    {
        if (project is null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Project} not found.");
        }

        if (string.IsNullOrWhiteSpace(project.Path))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Project} {project.Name} missing required property 'path'.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options)
    {
        var resourceOutputPath = Path.Combine(options.OutputPath, options.Resource.Key);

        _manifestWriter.EnsureOutputDirectoryExistsAndIsClean(resourceOutputPath);

        if (!_containerDetailsCache.TryGetValue(options.Resource.Key, out var containerDetails))
        {
            throw new InvalidOperationException($"Container details for project {options.Resource.Key} not found.");
        }

        var project = options.Resource.Value as ProjectResource;

        var data = PopulateKubernetesDeploymentData(options, containerDetails, project);

        var manifests = _manifests.ToList();
        if (data.IngressEnabled == true)
        {
            manifests.Add($"{TemplateLiterals.IngressType}.yaml");
            data.SetManifests(manifests);
        }

        _manifestWriter.CreateDeployment(resourceOutputPath, data, options.TemplatePath);
        _manifestWriter.CreateService(resourceOutputPath, data, options.TemplatePath);
        if (data.IngressEnabled == true)
        {
            _manifestWriter.CreateIngress(resourceOutputPath, data);
        }
        _manifestWriter.CreateComponentKustomizeManifest(resourceOutputPath, data, options.TemplatePath);

        LogCompletion(resourceOutputPath);

        return Task.FromResult(true);
    }

    private KubernetesDeploymentData PopulateKubernetesDeploymentData(BaseKubernetesCreateOptions options, MsBuildContainerProperties containerDetails, ProjectResource? project)
    {
        Dictionary<string, string>? annotations = project?.Annotations;
        List<string>? args = project?.Args;

        return new KubernetesDeploymentData()
            .SetWithDashboard(options.WithDashboard.GetValueOrDefault())
            .SetName(options.Resource.Key)
            .SetContainerImage(containerDetails.FullContainerImage)
            .SetImagePullPolicy(options.ImagePullPolicy)
            .SetEnv(GetFilteredEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetAnnotations(annotations)
            .SetArgs(args)
            .SetSecrets(GetSecretEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetSecretsFromSecretState(options.Resource, secretProvider, options.DisableSecrets)
            .SetIsProject(true)
            .SetPorts(options.Resource.MapBindingsToPorts())
            .SetManifests(_manifests)
            .SetDeployment((project as ProjectV1Resource)?.Deployment)
            .SetWithPrivateRegistry(options.WithPrivateRegistry.GetValueOrDefault())
            .ApplyAnnotations(options)
            .ApplySecurityContext(options)
            .ApplyIngress(options)
            .Validate();
    }

    public async Task BuildAndPushProjectContainer(KeyValuePair<string, Resource> resource, ContainerOptions options, bool nonInteractive, string? runtimeIdentifier, bool preferDockerfile, string? basePath = null)
    {
        var project = resource.Value as ProjectResource;

        if (!_containerDetailsCache.TryGetValue(resource.Key, out var containerDetails))
        {
            throw new InvalidOperationException($"Container details for project {resource.Key} not found.");
        }

        var dockerfileFile = !string.IsNullOrEmpty(containerDetails.DockerfileFile) ? containerDetails.DockerfileFile : Path.Combine(Path.GetDirectoryName(project.Path), "Dockerfile");

        if (preferDockerfile && File.Exists(dockerfileFile))
        {
            _console.MarkupLine($"[bold yellow]Using custom Dockerfile to build project {resource.Key}.[/]");

            var dockerfileResource = new DockerfileResource()
            {
                Path = dockerfileFile,
                Name = !string.IsNullOrEmpty(containerDetails.ContainerRepository) ? containerDetails.ContainerRepository : project.Name,
                Context = !string.IsNullOrEmpty(containerDetails.DockerfileContext) ? containerDetails.DockerfileContext : options.BuildContext,
                Bindings = project.Bindings,
                Env = project.Env,
                BuildArgs = options.BuildArgs
            };

            await containerCompositionService.BuildAndPushContainerForDockerfile(dockerfileResource, options, nonInteractive, basePath);
        }
        else
        {
            await containerCompositionService.BuildAndPushContainerForProject(project, containerDetails, options, nonInteractive, runtimeIdentifier, basePath);
        }

        _console.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Building and Pushing container for project [blue]{resource.Key}[/]");
    }

    public async Task PopulateContainerDetailsCacheForProject(KeyValuePair<string, Resource> resource, ContainerOptions options, string? basePath = null)
    {
        var project = resource.Value as ProjectResource;

        var details = await containerDetailsService.GetContainerDetails(resource.Key, project, options, basePath);

        var success = _containerDetailsCache.TryAdd(resource.Key, details);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to add container details for project {resource.Key} to cache.");
        }

        _console.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Populated container details cache for project [blue]{resource.Key}[/]");
    }

    public override ComposeService CreateComposeEntry(CreateComposeEntryOptions options)
    {
        var response = new ComposeService();

        if (!_containerDetailsCache.TryGetValue(options.Resource.Key, out var containerDetails))
        {
            throw new InvalidOperationException($"Container details for project {options.Resource.Key} not found.");
        }

        response.Service = Builder.MakeService(options.Resource.Key)
            .WithEnvironment(options.Resource.MapResourceToEnvVars(options.WithDashboard))
            .WithContainerName(options.Resource.Key)
            .WithRestartPolicy(ERestartMode.UnlessStopped)
            .WithPortMappings(options.Resource.MapBindingsToPorts().MapPortsToDockerComposePorts())
            .WithImage(containerDetails.FullContainerImage.ToLowerInvariant())
            .Build();

        response.IsProject = true;

        return response;
    }

    public override List<object> CreateKubernetesObjects(CreateKubernetesObjectsOptions options)
    {
        var project = options.Resource.Value as ProjectResource;

        if (!_containerDetailsCache.TryGetValue(options.Resource.Key, out var containerDetails))
        {
            throw new InvalidOperationException($"Container details for project {options.Resource.Key} not found.");
        }

        var data = PopulateKubernetesDeploymentData(options, containerDetails, project);

        return data.ToKubernetesObjects(options.EncodeSecrets);
    }

}
