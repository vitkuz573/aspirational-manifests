namespace Aspirate.Shared.Models.AspireManifests.Components.Aws;

/// <summary>
/// Represents an AWS CloudFormation stack resource.
/// </summary>
public class CloudFormationStackResource : Resource
{
    [JsonPropertyName("stack-name")]
    public string? StackName { get; set; }

    [JsonPropertyName("references")]
    public List<CloudFormationReference>? References { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
