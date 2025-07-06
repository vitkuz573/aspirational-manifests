using System.Text.Json.Serialization;
namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class BindMount : IJsonOnDeserialized
{
    /// <summary>
    /// Internal name used when generating manifests. Not part of the Aspire schema.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("readOnly")]
    public bool? ReadOnly { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"BindMount unexpected property '{unexpected}'.");
        }
    }
}

