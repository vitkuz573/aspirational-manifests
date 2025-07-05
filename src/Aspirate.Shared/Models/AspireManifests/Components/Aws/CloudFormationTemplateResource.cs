namespace Aspirate.Shared.Models.AspireManifests.Components.Aws;

/// <summary>
/// Represents an AWS CloudFormation template resource.
/// </summary>
public class CloudFormationTemplateResource : Resource
{
    [JsonPropertyName("stack-name")]
    public string? StackName { get; set; }

    [JsonPropertyName("template-path")]
    public string? TemplatePath { get; set; }

    [JsonPropertyName("references")]
    public Dictionary<string, string>? References { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
