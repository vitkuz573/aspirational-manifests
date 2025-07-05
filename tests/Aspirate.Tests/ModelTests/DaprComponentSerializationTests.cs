using System.Text.Json;
using Xunit;

namespace Aspirate.Tests.ModelTests;

public class DaprComponentSerializationTests
{
    [Fact]
    public void DaprComponentResource_RoundTrips_WithExtensionData()
    {
        var json = """
        {
            "type": "dapr.component.v0",
            "daprComponent": {
                "type": "state.redis",
                "version": "v1",
                "metadata": { "connectionString": "redis://localhost" }
            }
        }
        """;

        var resource = JsonSerializer.Deserialize<DaprComponentResource>(json)!;

        resource.DaprComponentProperty!.Type.Should().Be("state.redis");
        resource.DaprComponentProperty.Version.Should().Be("v1");
        resource.DaprComponentProperty.Metadata.Should().ContainKey("connectionString");

        var serialized = JsonSerializer.Serialize(resource);

        serialized.Should().Contain("\"version\":\"v1\"");
        serialized.Should().Contain("\"metadata\"");

        var roundTrip = JsonSerializer.Deserialize<DaprComponentResource>(serialized)!;
        roundTrip.DaprComponentProperty!.Version.Should().Be("v1");
        roundTrip.DaprComponentProperty.Metadata.Should().ContainKey("connectionString");
    }
}

