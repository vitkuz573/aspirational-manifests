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
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        ValidateStack(doc.RootElement);

        return doc.RootElement.Deserialize<CloudFormationStackResource>();
    }

    private static void ValidateStack(JsonElement element)
    {
        if (!element.TryGetProperty("stack-name", out var stackName) ||
            string.IsNullOrWhiteSpace(stackName.GetString()))
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.AwsCloudFormationStack} missing required property 'stack-name'.");
        }

        if (!element.TryGetProperty("template-path", out var templatePath) ||
            string.IsNullOrWhiteSpace(templatePath.GetString()))
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.AwsCloudFormationStack} missing required property 'template-path'.");
        }
    }

    /// <inheritdoc />
    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, these resources are preserved only.
        Task.FromResult(true);
}
