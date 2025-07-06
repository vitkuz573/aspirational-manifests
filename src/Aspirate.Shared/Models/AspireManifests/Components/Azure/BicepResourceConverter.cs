using System.Text.Json;
using System.Text.Json.Serialization;
using Aspirate.Shared.Literals;

namespace Aspirate.Shared.Models.AspireManifests.Components.Azure;

internal class BicepResourceConverter : JsonConverter<BicepResource>
{
    public override BicepResource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var newOptions = new JsonSerializerOptions(options);
        for (int i = newOptions.Converters.Count - 1; i >= 0; i--)
        {
            if (newOptions.Converters[i] is BicepResourceConverter)
            {
                newOptions.Converters.RemoveAt(i);
            }
        }
        if (!doc.RootElement.TryGetProperty("type", out var typeElement))
        {
            throw new InvalidOperationException("Deployment missing required property 'type'.");
        }

        var type = typeElement.GetString();
        return type switch
        {
            AspireComponentLiterals.AzureBicep => doc.RootElement.Deserialize<BicepResource>(newOptions),
            AspireComponentLiterals.AzureBicepV1 => doc.RootElement.Deserialize<BicepV1Resource>(newOptions),
            _ => throw new InvalidOperationException($"Deployment type '{type}' must be '{AspireComponentLiterals.AzureBicep}' or '{AspireComponentLiterals.AzureBicepV1}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, BicepResource value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        if (value.Path is not null)
        {
            writer.WriteString("path", value.Path);
        }
        if (value.ConnectionString is not null)
        {
            writer.WriteString("connectionString", value.ConnectionString);
        }
        if (value.Params is not null)
        {
            writer.WritePropertyName("params");
            JsonSerializer.Serialize(writer, value.Params, options);
        }

        if (value is BicepV1Resource v1 && v1.Scope is not null)
        {
            writer.WritePropertyName("scope");
            JsonSerializer.Serialize(writer, v1.Scope, options);
        }

        writer.WriteEndObject();
    }
}
