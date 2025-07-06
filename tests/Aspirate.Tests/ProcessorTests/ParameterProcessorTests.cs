using System;
using System.Text;
using Xunit;

namespace Aspirate.Tests.ProcessorTests;

public class ParameterProcessorTests
{
    [Fact]
    public void Deserialize_MissingInputType_Throws()
    {
        var processor = new ParameterProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"value\":\"x\",\"inputs\":{\"val\":{}}}";
        var bytes = Encoding.UTF8.GetBytes(json);
        Action act = () =>
        {
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            processor.Deserialize(ref reader);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*required property 'type'");
    }

    [Fact]
    public void Deserialize_DefaultWithBothGenerateAndValue_Throws()
    {
        var processor = new ParameterProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"value\":\"x\",\"inputs\":{\"val\":{\"type\":\"string\",\"default\":{\"value\":\"v\",\"generate\":{\"minLength\":5}}}}}";
        var bytes = Encoding.UTF8.GetBytes(json);
        Action act = () =>
        {
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            processor.Deserialize(ref reader);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*either 'generate' or 'value'");
    }

    [Fact]
    public void Deserialize_DefaultGenerateMinLengthZero_Throws()
    {
        var processor = new ParameterProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"value\":\"x\",\"inputs\":{\"val\":{\"type\":\"string\",\"default\":{\"generate\":{\"minLength\":0}}}}}";
        var bytes = Encoding.UTF8.GetBytes(json);
        Action act = () =>
        {
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            processor.Deserialize(ref reader);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*generate.minLength*");
    }
}
