namespace Aspirate.Processors.Resources.Aws;

/// <summary>
/// Processor for AWS CloudFormation stack resources.
/// </summary>
public class CloudFormationStackProcessor(IFileSystem fileSystem, IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.AwsCloudFormationStack;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<CloudFormationStackResource>(ref reader);

    /// <inheritdoc />
    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, these resources are preserved only.
        Task.FromResult(true);
}
