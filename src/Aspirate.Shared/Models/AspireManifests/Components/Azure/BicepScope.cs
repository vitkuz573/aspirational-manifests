using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Azure;

/// <summary>
/// Represents the scope for a bicep deployment.
/// </summary>
public class BicepScope : IJsonOnDeserialized
{
    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Bicep scope unexpected property '{unexpected}'.");
        }
    }
}

