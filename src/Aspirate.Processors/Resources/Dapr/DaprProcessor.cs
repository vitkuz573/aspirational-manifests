namespace Aspirate.Processors.Resources.Dapr;

public class DaprProcessor(
    IFileSystem fileSystem,
    IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    public override string ResourceType => AspireComponentLiterals.DaprSystem;

    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        var resource = JsonSerializer.Deserialize<DaprResource>(ref reader);
        ValidateDaprResource(resource, string.Empty);
        return resource;
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing for dapr, they are there for annotations on services.
        Task.FromResult(true);

    public override List<object> CreateKubernetesObjects(CreateKubernetesObjectsOptions options) => [];

    private static void ValidateDaprResource(DaprResource? resource, string name)
    {
        if (resource == null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.DaprSystem} {name} not found.");
        }

        if (resource.Metadata is null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.DaprSystem} {name} missing required property 'metadata'.");
        }

        if (string.IsNullOrWhiteSpace(resource.Metadata.Application))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.DaprSystem} {name} missing required property 'application'.");
        }

        if (string.IsNullOrWhiteSpace(resource.Metadata.AppId))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.DaprSystem} {name} missing required property 'appId'.");
        }

        if (resource.Metadata.Components is null || resource.Metadata.Components.Count == 0)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.DaprSystem} {name} missing required property 'components'.");
        }
    }

    public override ComposeService CreateComposeEntry(CreateComposeEntryOptions options)
    {
        var response = new ComposeService();

        var daprResource = options.Resource.Value as DaprResource;
        ValidateDaprResource(daprResource, options.Resource.Key);

        var commands = new List<string>
        {
            "./daprd",
            "-app-id",
            daprResource.Metadata.AppId,
        };

        var childResource = options.CurrentState.AllSelectedSupportedComponents.FirstOrDefault(x => x.Key == daprResource.Metadata.Application);

        if (childResource.Value is IResourceWithBinding childBinding)
        {
            var firstPort = childBinding.Bindings.FirstOrDefault();

            commands.Add("-app-port");
            commands.Add(firstPort.Value.Port?.ToString() ?? "8080");
        }

        response.Service = new Shared.Models.Compose.ComposeServiceBuilder()
            .WithName(options.Resource.Key)
            .WithImage("daprio/daprd:latest")
            .WithCommands(commands.ToArray())
            .WithDependencies(daprResource.Metadata.Application)
            .WithNetworkMode($"service:{childResource.Key}")
            .WithRestartPolicy(ERestartMode.UnlessStopped)
            .Build();

        return response;
    }
}
