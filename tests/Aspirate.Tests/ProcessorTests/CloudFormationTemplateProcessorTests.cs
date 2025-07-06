using System.IO;
using System.Text.Json;
using Aspirate.Processors.Resources.Aws;
using Aspirate.Shared.Inputs;
using Aspirate.Shared.Literals;
using Aspirate.Shared.Models.AspireManifests.Components.Aws;
using Xunit;

namespace Aspirate.Tests.ProcessorTests;

public class CloudFormationTemplateProcessorTests
{
    [Fact]
    public async Task CreateManifests_WritesTemplateFile()
    {
        var fs = new MockFileSystem();
        var writer = Substitute.For<IManifestWriter>();
        var processor = new CloudFormationTemplateProcessor(fs, Substitute.For<IAnsiConsole>(), writer);

        var resource = new CloudFormationTemplateResource { StackName = "demo", TemplatePath = "./tmpl.yml" };
        var options = new CreateManifestsOptions
        {
            Resource = new("tmpl", resource),
            OutputPath = "out",
            ImagePullPolicy = "IfNotPresent"
        };

        await processor.CreateManifests(options);

        var path = Path.Combine("out", "tmpl");

        writer.Received().EnsureOutputDirectoryExistsAndIsClean(path);
        writer.Received().CreateCustomManifest(path, $"{TemplateLiterals.CloudFormationTemplateType}.yaml", TemplateLiterals.CloudFormationTemplateType, resource, null);
    }
}

