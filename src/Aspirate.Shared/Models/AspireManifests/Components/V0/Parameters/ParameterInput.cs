namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

[ExcludeFromCodeCoverage]
public class ParameterInput : IJsonOnDeserialized
{
    private string? _type;

    [JsonPropertyName("type")]
    public string? Type
    {
        get => _type;
        set => _type = ValidateType(value);
    }

    [JsonPropertyName("default")]
    public ParameterDefault? Default { get; set; }

    [JsonPropertyName("secret")]
    public bool Secret { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    private static string? ValidateType(string? value)
    {
        if (value is null)
        {
            return null;
        }

        if (!string.Equals(value, ParameterInputLiterals.String, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Parameter input type must be '{ParameterInputLiterals.String}'.");
        }

        return value;
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is not null && AdditionalProperties.Count > 0)
        {
            var unexpected = AdditionalProperties.Keys.First();
            throw new InvalidOperationException($"Parameter input unexpected property '{unexpected}'.");
        }
    }
}
