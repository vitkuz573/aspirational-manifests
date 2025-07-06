using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class Build : IJsonOnDeserialized
{
    [JsonPropertyName("context")]
    public required string Context { get; set; }

    [JsonPropertyName("dockerfile")]
    public required string Dockerfile { get; set; }

    [JsonPropertyName("args")]
    public Dictionary<string, string>? Args { get; set; }

    [JsonPropertyName("secrets")]
    public Dictionary<string, BuildSecret>? Secrets { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Build unexpected property '{unexpected}'.");
        }
    }
}
