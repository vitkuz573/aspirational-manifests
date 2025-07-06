namespace Aspirate.Processors.Resources.Azure;

/// <summary>
/// Processor for azure bicep resources (v1).
/// </summary>
public class BicepV1Processor(IFileSystem fileSystem, IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.AzureBicepV1;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        var resource = JsonSerializer.Deserialize<BicepV1Resource>(ref reader);
        ValidateBicepResource(resource, string.Empty);
        return resource;
    }

    private static void ValidateBicepResource(BicepResource? resource, string name)
    {
        if (resource is null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.AzureBicepV1} {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(resource.Path))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.AzureBicepV1} {name} missing required property 'path'.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, bicep resources are preserved only.
        Task.FromResult(true);
}
