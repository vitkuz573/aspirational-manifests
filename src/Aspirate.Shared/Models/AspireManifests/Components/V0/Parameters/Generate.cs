namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

[ExcludeFromCodeCoverage]
public class Generate
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
}
