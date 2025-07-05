using System.Text.Json.Serialization;
namespace Aspirate.Shared.Models.AspireManifests.Components.V1.Project;

public class Deployment
{
    [JsonPropertyName("bindings")]
    public Dictionary<string, Binding>? Bindings { get; set; }

    [JsonPropertyName("annotations")]
    public Dictionary<string, string>? Annotations { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }
}
