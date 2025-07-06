using System.Text.Json;
using Aspirate.Processors.Resources.Aws;
using Xunit;

namespace Aspirate.Tests.ProcessorTests;

public class CloudFormationStackProcessorTests
{
    [Fact]
    public void Deserialize_AllowsMissingTemplatePath()
    {
        // Arrange
        var json = "{\"stack-name\":\"demo\"}";
        var processor = new CloudFormationStackProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); // start object

        // Act
        var resource = processor.Deserialize(ref reader) as CloudFormationStackResource;

        // Assert
        resource.StackName.Should().Be("demo");
        resource.References.Should().BeNull();
        resource.AdditionalProperties.Should().BeNull();
    }
}
