namespace Aspirate.Shared.Models.AspireManifests.Components.Azure;

/// <summary>
/// Represents an azure bicep deployment resource for manifest v1.
/// </summary>
public class BicepV1Resource : BicepResource
{
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
