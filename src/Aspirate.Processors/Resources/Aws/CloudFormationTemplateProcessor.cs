namespace Aspirate.Processors.Resources.Aws;

/// <summary>
/// Processor for AWS CloudFormation template resources.
/// </summary>
public class CloudFormationTemplateProcessor(IFileSystem fileSystem, IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.AwsCloudFormationTemplate;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<CloudFormationTemplateResource>(ref reader);

    /// <inheritdoc />
    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, these resources are preserved only.
        Task.FromResult(true);
}
