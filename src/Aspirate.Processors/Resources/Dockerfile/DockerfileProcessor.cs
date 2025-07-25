namespace Aspirate.Processors.Resources.Dockerfile;

/// <summary>
/// A project component for version 0 of Aspire.
/// </summary>
public class DockerfileProcessor(
    IFileSystem fileSystem,
    IAnsiConsole console,
    ISecretProvider secretProvider,
    IContainerCompositionService containerCompositionService,
    IContainerDetailsService containerDetailsService,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter), IImageProcessor
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.Dockerfile;

    private readonly IReadOnlyCollection<string> _manifests =
    [
        $"{TemplateLiterals.DeploymentType}.yaml",
        $"{TemplateLiterals.ServiceType}.yaml",
    ];

    private readonly Dictionary<string, List<string>> _containerImageCache = [];

    private static void ValidateDockerfile(DockerfileResource? dockerfile, string name)
    {
        if (dockerfile == null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Dockerfile} {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(dockerfile.Path))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Dockerfile} {name} missing required property 'path'.");
        }

        if (string.IsNullOrWhiteSpace(dockerfile.Context))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Dockerfile} {name} missing required property 'context'.");
        }
    }

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<DockerfileResource>(ref reader);

    public override Task<bool> CreateManifests(CreateManifestsOptions options)
    {
        var resourceOutputPath = Path.Combine(options.OutputPath, options.Resource.Key);

        _manifestWriter.EnsureOutputDirectoryExistsAndIsClean(resourceOutputPath);

        var dockerFile = options.Resource.Value as DockerfileResource;
        ValidateDockerfile(dockerFile, options.Resource.Key);

        if (!_containerImageCache.TryGetValue(options.Resource.Key, out var containerImages))
        {
            throw new InvalidOperationException($"Container Image for dockerfile {options.Resource.Key} not found.");
        }

        var data = PopulateKubernetesDeploymentData(options, containerImages.First(), dockerFile);

        _manifestWriter.CreateDeployment(resourceOutputPath, data, options.TemplatePath);
        _manifestWriter.CreateService(resourceOutputPath, data, options.TemplatePath);
        _manifestWriter.CreateComponentKustomizeManifest(resourceOutputPath, data, options.TemplatePath);

        LogCompletion(resourceOutputPath);

        return Task.FromResult(true);
    }

    private KubernetesDeploymentData PopulateKubernetesDeploymentData(BaseKubernetesCreateOptions options, string containerImage, DockerfileResource? dockerFile) =>
        new KubernetesDeploymentData()
            .SetWithDashboard(options.WithDashboard.GetValueOrDefault())
            .SetName(options.Resource.Key)
            .SetContainerImage(containerImage)
            .SetImagePullPolicy(options.ImagePullPolicy)
            .SetEnv(GetFilteredEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetSecrets(GetSecretEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetSecretsFromSecretState(options.Resource, secretProvider, options.DisableSecrets)
            .SetPorts(options.Resource.MapBindingsToPorts())
            .SetManifests(_manifests)
            .SetWithPrivateRegistry(options.WithPrivateRegistry.GetValueOrDefault())
            .ApplySecurityContext(options)
            .Validate();

    public async Task BuildAndPushContainerForDockerfile(KeyValuePair<string, Resource> resource, ContainerOptions options, bool nonInteractive, string? basePath = null)
    {
        var dockerfile = resource.Value as DockerfileResource;
        ValidateDockerfile(dockerfile, resource.Key);

        await containerCompositionService.BuildAndPushContainerForDockerfile(dockerfile, options, nonInteractive, basePath);

        _console.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Building and Pushing container for Dockerfile [blue]{resource.Key}[/]");
    }

    public void PopulateContainerImageCacheWithImage(KeyValuePair<string, Resource> resource, ContainerOptions options)
    {
        _containerImageCache.Add(resource.Key, options.ToImageNames(resource.Key));

        _console.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Setting container details for Dockerfile [blue]{resource.Key}[/]");
    }

    public override ComposeService CreateComposeEntry(CreateComposeEntryOptions options)
    {
        var response = new ComposeService();

        var dockerfile = options.Resource.Value as DockerfileResource;
        ValidateDockerfile(dockerfile, options.Resource.Key);

        var newService = Builder.MakeService(options.Resource.Key)
            .WithEnvironment(options.Resource.MapResourceToEnvVars(options.WithDashboard))
            .WithContainerName(options.Resource.Key)
            .WithRestartPolicy(ERestartMode.UnlessStopped)
            .WithPortMappings(options.Resource.MapBindingsToPorts().MapPortsToDockerComposePorts());

            if (options.ComposeBuilds == true)
            {
                newService = newService.WithBuild(builder =>
                {
                builder.WithContext(_fileSystem.GetFullPath(dockerfile.Context, options.CurrentState?.ManifestDirectory))
                    .WithDockerfile(_fileSystem.GetFullPath(dockerfile.Path, options.CurrentState?.ManifestDirectory))
                    .Build();
                });
            }
        else
        {
            if (!_containerImageCache.TryGetValue(options.Resource.Key, out var containerImage))
            {
                throw new InvalidOperationException($"Container Image for dockerfile {options.Resource.Key} not found.");
            }

            newService = newService.WithImage(containerImage[0].ToLowerInvariant());
        }

        response.Service = newService.Build();

        return response;
    }

    public override List<object> CreateKubernetesObjects(CreateKubernetesObjectsOptions options)
    {
        var dockerFile = options.Resource.Value as DockerfileResource;

        if (!_containerImageCache.TryGetValue(options.Resource.Key, out var containerImage))
        {
            throw new InvalidOperationException($"Container Image for dockerfile {options.Resource.Key} not found.");
        }

        var data = PopulateKubernetesDeploymentData(options, containerImage[0], dockerFile);

        return data.ToKubernetesObjects(options.EncodeSecrets);
    }
}
