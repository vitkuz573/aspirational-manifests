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

        if (element.TryGetProperty("references", out var references) &&
            references.ValueKind == JsonValueKind.Array)
        {
            foreach (var reference in references.EnumerateArray())
            {
                if (!reference.TryGetProperty("target-resource", out var targetResource) ||
                    string.IsNullOrWhiteSpace(targetResource.GetString()))
                {
                    throw new InvalidOperationException(
                        $"{AspireComponentLiterals.AwsCloudFormationStack} reference missing required property 'target-resource'.");
                }
            }
        }
    }

    /// <inheritdoc />
    public override Task<bool> CreateManifests(CreateManifestsOptions options)
    {
        var resourceOutputPath = Path.Combine(options.OutputPath, options.Resource.Key);

        _manifestWriter.EnsureOutputDirectoryExistsAndIsClean(resourceOutputPath);

        var stack = options.Resource.Value as CloudFormationStackResource;

        _manifestWriter.CreateCustomManifest(
            resourceOutputPath,
            $"{TemplateLiterals.CloudFormationStackType}.yaml",
            TemplateLiterals.CloudFormationStackType,
            stack!,
            options.TemplatePath);

        LogCompletion(resourceOutputPath);

        return Task.FromResult(true);
    }
}
