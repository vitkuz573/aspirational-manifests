using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

public enum BuildSecretType
{
    [EnumMember(Value = "env")]
    Env,

    [EnumMember(Value = "file")]
    File,
}

public class BuildSecret
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required BuildSecretType Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
