namespace Aspirate.Shared.Models.AspireManifests.Components.Azure;

/// <summary>
/// Represents the scope for a bicep deployment.
/// </summary>
public class BicepScope
{
    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }
}

