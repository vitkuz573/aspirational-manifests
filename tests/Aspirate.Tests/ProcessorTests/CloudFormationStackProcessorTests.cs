using System.Text.Json;
using System.IO;
using Aspirate.Shared.Literals;
using Aspirate.Shared.Inputs;
using Aspirate.Shared.Models.AspireManifests.Components.Aws;
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

    [Fact]
    public async Task CreateManifests_WritesStackFile()
    {
        var fs = new MockFileSystem();
        var writer = Substitute.For<IManifestWriter>();
        var processor = new CloudFormationStackProcessor(fs, Substitute.For<IAnsiConsole>(), writer);

        var resource = new CloudFormationStackResource { StackName = "demo" };
        var options = new CreateManifestsOptions
        {
            Resource = new("stack", resource),
            OutputPath = "out",
            ImagePullPolicy = "IfNotPresent"
        };

        await processor.CreateManifests(options);

        var path = Path.Combine("out", "stack");

        writer.Received().EnsureOutputDirectoryExistsAndIsClean(path);
        writer.Received().CreateCustomManifest(path, $"{TemplateLiterals.CloudFormationStackType}.yaml", TemplateLiterals.CloudFormationStackType, resource, null);
    }
}
