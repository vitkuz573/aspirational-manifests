using System.Text.Json;
using Xunit;

namespace Aspirate.Tests.ModelTests;

public class BindingSerializationTests
{
    [Fact]
    public void Binding_Serializes_UsingSchemeProperty()
    {
        var binding = new Binding { Scheme = "https" };
        var json = JsonSerializer.Serialize(binding);
        json.Should().Contain("\"scheme\":\"https\"");
    }
}
