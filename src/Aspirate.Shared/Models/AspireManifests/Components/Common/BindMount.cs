using System.Text.Json.Serialization;
namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class BindMount
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("readOnly")]
    public bool ReadOnly { get; set; }
}

