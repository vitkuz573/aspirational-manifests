using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.Aspirate;

/// <summary>
/// Represents basic pod security context options persisted in state.
/// </summary>
public class PodSecurityContext
{
    [JsonPropertyName("runAsUser")]
    public long? RunAsUser { get; set; }

    [JsonPropertyName("runAsGroup")]
    public long? RunAsGroup { get; set; }

    [JsonPropertyName("fsGroup")]
    public long? FsGroup { get; set; }

    [JsonPropertyName("runAsNonRoot")]
    public bool? RunAsNonRoot { get; set; }
}
