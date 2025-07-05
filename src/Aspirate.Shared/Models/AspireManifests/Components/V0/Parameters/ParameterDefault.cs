namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

public class ParameterDefault
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("generate")]
    public Generate? Generate { get; set; }
}
