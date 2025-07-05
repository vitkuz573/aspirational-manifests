using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
namespace Aspirate.Shared.Models.AspireManifests.Components;

/// <summary>
/// Represents a custom resource type not natively supported.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExtensionResource : Resource
{
    public ExtensionResource(JsonNode rawJson, string type)
    {
        Type = type;
        RawJson = rawJson;
    }

    [JsonIgnore]
    public JsonNode RawJson { get; }
}
