namespace Aspirate.Processors.Resources.AbstractProcessors;


/// <summary>
/// A base container component shared between Aspire version 0 and 1.
/// </summary>
public abstract class BaseContainerProcessor<TContainerResource>(
    IFileSystem fileSystem,
    IAnsiConsole console,
    ISecretProvider secretProvider,
    IContainerCompositionService containerCompositionService,
    IContainerDetailsService containerDetailsService,
    IManifestWriter manifestWriter)
        : BaseResourceProcessor(fileSystem, console, manifestWriter), IImageProcessor
        where TContainerResource : ContainerResourceBase
{
    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<TContainerResource>(ref reader);

    private readonly Dictionary<string, List<string>> _containerImageCache = [];

    private static void ValidateContainerResource(TContainerResource? container, string name)
    {
        if (container is null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Container} {name} not found.");
        }

        if (container is ContainerResource containerV0 && string.IsNullOrWhiteSpace(containerV0.Image))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Container} {name} missing required property 'image'.");
        }

        if (container is ContainerV1Resource containerV1)
        {
            if (containerV1.Image is null && containerV1.Build is null)
            {
                throw new InvalidOperationException($"{AspireComponentLiterals.ContainerV1} must have image or build property.");
            }

            if (containerV1.Build is not null)
            {
                if (string.IsNullOrWhiteSpace(containerV1.Build.Context))
                {
                    throw new InvalidOperationException($"{AspireComponentLiterals.ContainerV1} {name} missing required build context.");
                }

                if (string.IsNullOrWhiteSpace(containerV1.Build.Dockerfile))
                {
                    throw new InvalidOperationException($"{AspireComponentLiterals.ContainerV1} {name} missing required build dockerfile.");
                }
            }
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options)
    {
        var resourceOutputPath = Path.Combine(options.OutputPath, options.Resource.Key);

        _manifestWriter.EnsureOutputDirectoryExistsAndIsClean(resourceOutputPath);

        var container = options.Resource.Value as TContainerResource;
        ValidateContainerResource(container, options.Resource.Key);

        var manifests = new List<string>
        {
            container.Volumes.Count > 0
                ? $"{TemplateLiterals.StatefulSetType}.yaml"
                : $"{TemplateLiterals.DeploymentType}.yaml",
            $"{TemplateLiterals.ServiceType}.yaml",
        };

        var image = GetImageFromContainerResource(options.Resource);
        var data = PopulateKubernetesDeploymentData(options, image, container, manifests);

        if (container.Volumes.Count > 0)
        {
            _manifestWriter.CreateStatefulSet(resourceOutputPath, data, options.TemplatePath);
        }
        else
        {
            _manifestWriter.CreateDeployment(resourceOutputPath, data, options.TemplatePath);
        }

        _manifestWriter.CreateService(resourceOutputPath, data, options.TemplatePath);
        _manifestWriter.CreateComponentKustomizeManifest(resourceOutputPath, data, options.TemplatePath);

        LogCompletion(resourceOutputPath);

        return Task.FromResult(true);
    }

    private string GetImageFromContainerResource(KeyValuePair<string, Resource> resource)
    {
        switch (resource.Value)
        {
            case ContainerResource containerV0:
                return containerV0.Image;

            case ContainerV1Resource containerV1:
                if (containerV1.Image != null)
                {
                    return containerV1.Image;
                }
                else if (containerV1.Build != null)
                {
                    return GetCachedImages(resource.Key).First();
                }
                else
                {
                    throw new InvalidOperationException($"{AspireComponentLiterals.ContainerV1} must have image or build property.");
                }

            default:
                throw new InvalidOperationException($"Unexpected resource type {resource.Value?.GetType().Name}");
        }
    }

    private List<string> GetCachedImages(string key) =>
        _containerImageCache.TryGetValue(key, out var containerImages) ?
            containerImages :
        throw new InvalidOperationException($"Container Image for {key} not found.");

    private KubernetesDeploymentData PopulateKubernetesDeploymentData(BaseKubernetesCreateOptions options, string image, TContainerResource? container, List<string> manifests) =>
        new KubernetesDeploymentData()
            .SetWithDashboard(options.WithDashboard.GetValueOrDefault())
            .SetName(options.Resource.Key)
            .SetContainerImage(image)
            .SetImagePullPolicy(options.ImagePullPolicy)
            .SetEnv(GetFilteredEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetAnnotations(container.Annotations)
            .SetVolumes(container.Volumes.KuberizeVolumeNames(options.Resource))
            .SetBindMounts(container.BindMounts.KuberizeBindMountNames(options.Resource))
            .SetSecrets(GetSecretEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetSecretsFromSecretState(options.Resource, secretProvider, options.DisableSecrets)
            .SetPorts(options.Resource.MapBindingsToPorts())
            .SetArgs(container.Args)
            .SetEntrypoint(container.Entrypoint)
            .SetManifests(manifests)
            .SetDeployment((container as ContainerV1Resource)?.Deployment)
            .SetWithPrivateRegistry(options.WithPrivateRegistry.GetValueOrDefault())
            .ApplyIngress(options)
            .Validate();

    public async Task BuildAndPushContainerForDockerfile(KeyValuePair<string, Resource> resource, ContainerOptions options, bool nonInteractive)
    {
        if (resource.Value is not ContainerV1Resource containerV1 || containerV1.Build == null)
        {
            return;
        }

        ValidateContainerResource(containerV1, resource.Key);

        await containerCompositionService.BuildAndPushContainerForDockerfile(containerV1, options, nonInteractive);

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

        var container = options.Resource.Value as TContainerResource;
        ValidateContainerResource(container, options.Resource.Key);

        var service = Builder.MakeService(options.Resource.Key);

        if (container is ContainerResource containerV0)
        {
            service.WithImage(containerV0.Image.ToLowerInvariant());
        }
        else if (container is ContainerV1Resource containerV1)
        {
            if (containerV1.Image != null)
            {
                service.WithImage(containerV1.Image.ToLowerInvariant());
            }
            else if (containerV1.Build != null && options.ComposeBuilds == true)
            {
                service.WithBuild(builder =>
                    builder
                        .WithContext(_fileSystem.GetFullPath(containerV1.Build.Context))
                        .WithDockerfile(_fileSystem.GetFullPath(containerV1.Build.Dockerfile))
                        .Build());
            }
        }

        if (container.Args is not null)
        {
            service.WithCommands(container.Args.ToArray());
        }

        var newService = service
            .WithEnvironment(options.Resource.MapResourceToEnvVars(options.WithDashboard))
            .WithContainerName(options.Resource.Key);

        if (!string.IsNullOrEmpty(container.Entrypoint))
        {
            newService = newService.WithCommands(container.Entrypoint);
        }

        response.Service = newService.WithRestartPolicy(ERestartMode.UnlessStopped)
            .WithVolumes(options.Resource.MapComposeVolumes())
            .WithPortMappings(options.Resource.MapBindingsToPorts().MapPortsToDockerComposePorts())
            .Build();

        return response;
    }

    public override List<object> CreateKubernetesObjects(CreateKubernetesObjectsOptions options)
    {
        var container = options.Resource.Value as TContainerResource;
        ValidateContainerResource(container, options.Resource.Key);
        var image = GetImageFromContainerResource(options.Resource);
        var data = PopulateKubernetesDeploymentData(options, image, container, []);

        return data.ToKubernetesObjects(options.EncodeSecrets);
    }

}
