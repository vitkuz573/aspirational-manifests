namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Dapr;

public sealed class InnerDaprComponent : IJsonOnDeserialized
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    [JsonIgnore]
    public string? Version
    {
        get
        {
            if (AdditionalProperties is null)
            {
                return null;
            }

            return AdditionalProperties.TryGetValue("version", out var element) &&
                   element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;
        }
        set
        {
            if (value is null)
            {
                AdditionalProperties?.Remove("version");
                return;
            }

            AdditionalProperties ??= new();
            AdditionalProperties["version"] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();
        }
    }

    [JsonIgnore]
    public Dictionary<string, string>? Metadata
    {
        get
        {
            if (AdditionalProperties is null ||
                !AdditionalProperties.TryGetValue("metadata", out var element) ||
                element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var dict = new Dictionary<string, string>();
            foreach (var prop in element.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.ToString();
            }

            return dict;
        }
        set
        {
            if (value is null)
            {
                AdditionalProperties?.Remove("metadata");
                return;
            }

            AdditionalProperties ??= new();
            AdditionalProperties["metadata"] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();
        }
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (AdditionalProperties is null)
        {
            return;
        }

        foreach (var key in AdditionalProperties.Keys)
        {
            if (string.Equals(key, "version", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "metadata", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            throw new InvalidOperationException($"Dapr component unexpected property '{key}'.");
        }
    }
}
