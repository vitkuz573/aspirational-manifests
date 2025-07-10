using Xunit;

namespace Aspirate.Tests.ModelTests;

public class ParameterInputSerializationTests
{
    [Theory]
    [InlineData("number")]
    [InlineData("bool")]
    public void Setting_Invalid_Type_Throws(string value)
    {
        var input = new ParameterInput();
        var act = () => input.Type = value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Parameter input type must be 'string'.");
    }

    [Fact]
    public void Deserializing_Invalid_Type_Throws()
    {
        var json = "{\"type\":\"number\"}";

        var act = () => JsonSerializer.Deserialize<ParameterInput>(json);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Parameter input type must be 'string'.");
    }

    [Fact]
    public void Deserializing_With_Unknown_Property_Throws()
    {
        var json = "{\"type\":\"string\",\"extra\":true}";

        var act = () => JsonSerializer.Deserialize<ParameterInput>(json);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Parameter input unexpected property 'extra'.");
    }
}
