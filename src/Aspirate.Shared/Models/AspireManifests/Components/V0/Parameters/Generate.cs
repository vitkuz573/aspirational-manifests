using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

[ExcludeFromCodeCoverage]
public class Generate : IJsonOnDeserialized
{
    [JsonPropertyName("minLength")]
    public int MinLength { get; set; }

    [JsonPropertyName("lower")]
    public bool Lower { get; set; } = true;

    [JsonPropertyName("upper")]
    public bool Upper { get; set; } = true;

    [JsonPropertyName("numeric")]
    public bool Numeric { get; set; } = true;

    [JsonPropertyName("special")]
    public bool Special { get; set; } = true;

    [JsonPropertyName("minLower")]
    public int MinLower { get; set; }

    [JsonPropertyName("minUpper")]
    public int MinUpper { get; set; }

    [JsonPropertyName("minNumeric")]
    public int MinNumeric { get; set; }

    [JsonPropertyName("minSpecial")]
    public int MinSpecial { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Generate unexpected property '{unexpected}'.");
        }
    }
}

