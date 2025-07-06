namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class Volume : IJsonOnDeserialized
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

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
            throw new InvalidOperationException($"Volume unexpected property '{unexpected}'.");
        }
    }
}
