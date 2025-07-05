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
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<BicepResource>(ref reader);

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, bicep resources are preserved only.
        Task.FromResult(true);
}
