using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class Build
{
    [JsonPropertyName("context")]
    public required string Context { get; set; }

    [JsonPropertyName("dockerfile")]
    public required string Dockerfile { get; set; }

    [JsonPropertyName("args")]
    public Dictionary<string, string>? Args { get; set; }

    [JsonPropertyName("secrets")]
    public Dictionary<string, BuildSecret>? Secrets { get; set; }
}
