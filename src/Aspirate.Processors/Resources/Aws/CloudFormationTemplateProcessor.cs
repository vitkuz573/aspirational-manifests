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
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        ValidateTemplate(doc.RootElement);

        return doc.RootElement.Deserialize<CloudFormationTemplateResource>();
    }

    private static void ValidateTemplate(JsonElement element)
    {
        if (!element.TryGetProperty("stack-name", out var stackName) ||
            string.IsNullOrWhiteSpace(stackName.GetString()))
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.AwsCloudFormationTemplate} missing required property 'stack-name'.");
        }

        if (!element.TryGetProperty("template-path", out var templatePath) ||
            string.IsNullOrWhiteSpace(templatePath.GetString()))
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.AwsCloudFormationTemplate} missing required property 'template-path'.");
        }

        if (element.TryGetProperty("references", out var references) &&
            references.ValueKind == JsonValueKind.Array)
        {
            foreach (var reference in references.EnumerateArray())
            {
                if (!reference.TryGetProperty("target-resource", out var targetResource) ||
                    string.IsNullOrWhiteSpace(targetResource.GetString()))
                {
                    throw new InvalidOperationException(
                        $"{AspireComponentLiterals.AwsCloudFormationTemplate} reference missing required property 'target-resource'.");
                }
            }
        }
    }

    /// <inheritdoc />
    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing, these resources are preserved only.
        Task.FromResult(true);
}
