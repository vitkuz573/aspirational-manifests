namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Dapr;

public class Metadata
{
    [JsonPropertyName("application")]
    public string Application { get; set; } = default!;

    [JsonPropertyName("appId")]
    public string AppId { get; set; } = default!;

    [JsonPropertyName("components")]
    public List<string>? Components { get; set; }

    /// <summary>
    /// Holds any additional properties that may be present in the manifest but
    /// are not part of the Aspire 8.0 schema.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
