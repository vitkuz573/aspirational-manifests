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

    [Theory]
    [InlineData("bad")]
    [InlineData("ftp")]
    public void Setting_Invalid_Scheme_Throws(string value)
    {
        var binding = new Binding();
        var act = () => binding.Scheme = value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("bad")]
    [InlineData("icmp")]
    public void Setting_Invalid_Protocol_Throws(string value)
    {
        var binding = new Binding();
        var act = () => binding.Protocol = value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("bad")]
    [InlineData("spdy")]
    public void Setting_Invalid_Transport_Throws(string value)
    {
        var binding = new Binding();
        var act = () => binding.Transport = value;

        act.Should().Throw<InvalidOperationException>();
    }
}
