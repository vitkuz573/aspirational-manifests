namespace Aspirate.Processors.Resources.Azure;

/// <summary>
/// Processor for azure bicep resources (v0).
/// </summary>
public class BicepProcessor(IFileSystem fileSystem, IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.AzureBicep;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        var resource = JsonSerializer.Deserialize<BicepResource>(ref reader);
        ValidateBicepResource(resource, string.Empty);
        return resource;
    }

    private static void ValidateBicepResource(BicepResource? resource, string name)
    {
        if (resource is null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.AzureBicep} {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(resource.Path))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.AzureBicep} {name} missing required property 'path'.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, bicep resources are preserved only.
        Task.FromResult(true);
}
