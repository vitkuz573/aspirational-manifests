using System.Text.Json;
using Aspirate.Shared.Models.AspireManifests.Components.Azure;
using FluentAssertions;
using Xunit;

namespace Aspirate.Tests.ModelTests;

public class BicepResourceSerializationTests
{
    [Fact]
    public void BicepResource_Params_RoundTrips_MultiTypes()
    {
        // Arrange
        var json = """
        {
            "number": 1,
            "array": [1, 2, 3],
            "object": { "a": true }
        }
        """;
        using var doc = JsonDocument.Parse(json);
        var dict = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.Clone());

        var resource = new BicepResource
        {
            Params = dict
        };

        // Act
        var serialized = JsonSerializer.Serialize(resource);
        var deserialized = JsonSerializer.Deserialize<BicepResource>(serialized)!;

        // Assert
        deserialized.Params.Should().ContainKey("number");
        deserialized.Params!["number"].ValueKind.Should().Be(JsonValueKind.Number);
        deserialized.Params["number"].GetInt32().Should().Be(1);

        deserialized.Params.Should().ContainKey("array");
        deserialized.Params["array"].ValueKind.Should().Be(JsonValueKind.Array);
        deserialized.Params["array"].EnumerateArray().Select(e => e.GetInt32()).Should().Equal(1, 2, 3);

        deserialized.Params.Should().ContainKey("object");
        deserialized.Params["object"].ValueKind.Should().Be(JsonValueKind.Object);
        deserialized.Params["object"].GetProperty("a").GetBoolean().Should().BeTrue();
    }
}
