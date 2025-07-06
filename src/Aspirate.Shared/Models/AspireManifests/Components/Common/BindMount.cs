using System.Text.Json.Serialization;
namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public class BindMount
{
    /// <summary>
    /// Internal name used when generating manifests. Not part of the Aspire schema.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("readOnly")]
    public bool ReadOnly { get; set; }
}

