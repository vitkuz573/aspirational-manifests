using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public enum BuildSecretType
{
    [EnumMember(Value = AspirateSecretLiterals.EnvironmentSecretsManager)]
    Env,

    [EnumMember(Value = AspirateSecretLiterals.FileSecretsManager)]
    File,
}

public class BuildSecret : IJsonOnDeserialized
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required BuildSecretType Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Build secret unexpected property '{unexpected}'.");
        }
    }
}
