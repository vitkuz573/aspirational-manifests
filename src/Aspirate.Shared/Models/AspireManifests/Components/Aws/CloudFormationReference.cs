namespace Aspirate.Shared.Models.AspireManifests.Components.Aws;

/// <summary>
/// Represents a reference to another resource for CloudFormation operations.
/// </summary>
public class CloudFormationReference
{
    [JsonPropertyName("target-resource")]
    public string? TargetResource { get; set; }
}
