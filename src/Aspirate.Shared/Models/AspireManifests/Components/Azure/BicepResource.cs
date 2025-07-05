using System.Text.Json;

namespace Aspirate.Shared.Models.AspireManifests.Components.Azure;

/// <summary>
/// Represents an azure bicep deployment resource for manifest v0.
/// </summary>
public class BicepResource : Resource, IResourceWithConnectionString
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    [JsonPropertyName("params")]
    public Dictionary<string, JsonElement>? Params { get; set; }
}
