using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

public class ParameterDefault : IJsonOnDeserialized
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("generate")]
    public Generate? Generate { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Parameter default unexpected property '{unexpected}'.");
        }
    }
}
