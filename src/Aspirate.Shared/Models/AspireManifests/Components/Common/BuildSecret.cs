using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class BuildSecret
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
