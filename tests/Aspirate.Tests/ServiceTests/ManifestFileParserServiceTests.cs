using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspirate.Cli;
using Aspirate.Shared.Models.AspireManifests.Components.Azure;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class ManifestFileParserServiceTest
{
    [Fact]
    public void LoadAndParseAspireManifest_ShouldCallExpectedMethods_WhenCalled()
    {
        // Arrange

        var fileSystem = new MockFileSystem();
        var manifestFile = "testManifest.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void LoadAndParseAspireManifest_SetsManifestDirectory()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("/manifests/manifest.json", new("{\"resources\": {}}"));
        fileSystem.Directory.SetCurrentDirectory("/other");
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        service.LoadAndParseAspireManifest("/manifests/manifest.json");

        service.ManifestDirectory.Should().Be("/manifests");
    }

    [Fact]
    public void LoadAndParseAspireManifest_ThrowsException_WhenManifestFileDoesNotExist()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest("nonexistent.json");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The manifest file could not be loaded from: 'nonexistent.json'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_ThrowsException_WhenResourcesObjectMissing()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "empty.json";
        fileSystem.AddFile(manifestFile, new("{}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The manifest file does not contain a 'resources' object.");
    }

    [Fact]
    public void LoadAndParseAspireManifest_ReturnsUnsupportedResource_WhenResourceTypeIsMissing()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "missingType.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"resource1\": {}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        result.Should().HaveCount(1);
        result["resource1"].Should().BeOfType<UnsupportedResource>();
    }

    [Fact]
    public void LoadAndParseAspireManifest_ReturnsExtensionResource_WhenResourceTypeIsUnsupported()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "unsupportedType.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"resource1\": {\"type\": \"unsupported\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        result.Should().HaveCount(1);
        result["resource1"].Should().BeOfType<ExtensionResource>();
    }

    [Fact]
    public void LoadAndParseAspireManifest_PreservesUnknownResourceTypeJson()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "unknown.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"res\": {\"type\": \"custom.v0\", \"foo\": \"bar\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        result.Should().HaveCount(1);
        var ext = result["res"].As<ExtensionResource>();
        ext.Type.Should().Be("custom.v0");
        ext.RawJson!["foo"]!.ToString().Should().Be("bar");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenUnknownAwsProperty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "aws.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"stack\": {\"type\": \"aws.cloudformation.stack.v0\", \"stack-name\": \"demo\", \"template-path\": \"./stack.yml\", \"extra\": \"val\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_UnknownContainerProperty_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "annotations.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v0\", \"image\": \"img\", \"annotations\": {\"key\": \"val\"}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'annotations'*");
    }

    [Fact]
    public void LoadAndParseAspireManifest_UnknownDockerfileProperty_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "dockerfile-unknown.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"dockerfile.v0\", \"path\": \"Dockerfile\", \"context\": \"./\", \"args\": []}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'args'*");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenBuildHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "build-extra.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v1\", \"build\": {\"context\": \"./\", \"dockerfile\": \"Dockerfile\", \"extra\": true}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenBuildSecretHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "build-secret-extra.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v1\", \"build\": {\"context\": \"./\", \"dockerfile\": \"Dockerfile\", \"secrets\": {\"MY_SECRET\": {\"type\": \"env\", \"extra\": true}}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenBuildFileSecretHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "build-file-secret-extra.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v1\", \"build\": {\"context\": \"./\", \"dockerfile\": \"Dockerfile\", \"secrets\": {\"MY_SECRET\": {\"type\": \"file\", \"source\": \"./secret.txt\", \"extra\": true}}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenVolumeHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "volume-extra.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v1\", \"image\": \"img\", \"volumes\": [{\"name\": \"data\", \"target\": \"/data\", \"readOnly\": false, \"extra\": 1}]}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenBindMountHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "bindmount-extra.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"svc\": {\"type\": \"container.v1\", \"image\": \"img\", \"bindMounts\": [{\"source\": \"./src\", \"target\": \"/data\", \"readOnly\": true, \"extra\": 1}]}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenCloudFormationStackMissingStackName()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "missing-stack-name.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"stack\": {\"type\": \"aws.cloudformation.stack.v0\", \"template-path\": \"./stack.yml\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'stack-name'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenCloudFormationTemplateMissingTemplatePath()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "missing-template-path.json";
        fileSystem.AddFile(manifestFile, new("{\"resources\": {\"tmpl\": {\"type\": \"aws.cloudformation.template.v0\", \"stack-name\": \"demo\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'template-path'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Parses_CloudFormationStackReferences()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "stack-ref.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"stack\": {\"type\": \"aws.cloudformation.stack.v0\", \"stack-name\": \"demo\", \"template-path\": \"./stack.yml\", \"references\": [{\"target-resource\": \"app\"}]}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        var stack = result["stack"].As<CloudFormationStackResource>();
        stack.References.Should().ContainSingle();
        stack.References![0].TargetResource.Should().Be("app");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Parses_CloudFormationTemplateReferences()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "template-ref.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"tmpl\": {\"type\": \"aws.cloudformation.template.v0\", \"stack-name\": \"demo\", \"template-path\": \"./tmpl.yml\", \"references\": [{\"target-resource\": \"app\"}]}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        var template = result["tmpl"].As<CloudFormationTemplateResource>();
        template.References.Should().ContainSingle();
        template.References![0].TargetResource.Should().Be("app");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenCloudFormationStackReferenceMissingTarget()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "stack-ref-missing-target.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"stack\": {\"type\": \"aws.cloudformation.stack.v0\", \"stack-name\": \"demo\", \"template-path\": \"./stack.yml\", \"references\": [{ }]}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*reference missing required property 'target-resource'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenCloudFormationTemplateReferenceMissingTarget()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "template-ref-missing-target.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"tmpl\": {\"type\": \"aws.cloudformation.template.v0\", \"stack-name\": \"demo\", \"template-path\": \"./tmpl.yml\", \"references\": [{ }]}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*reference missing required property 'target-resource'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenParameterMissingValue()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "missing-parameter-value.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"inputs\": {\"value\": {}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'value'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenParameterMissingInputs()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "missing-parameter-inputs.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"value\": \"foo\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'inputs'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenParameterInputHasUnknownProperty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "parameter-input-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"value\": \"foo\", \"inputs\": {\"value\": {\"type\": \"string\", \"extra\": true}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<JsonException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenParameterDefaultHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "parameter-default-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"value\": \"foo\", \"inputs\": {\"value\": {\"type\": \"string\", \"default\": {\"value\": \"bar\", \"extra\": true}}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenParameterDefaultGenerateHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "parameter-default-generate-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"value\": \"foo\", \"inputs\": {\"value\": {\"type\": \"string\", \"default\": {\"generate\": {\"minLength\": 8}, \"extra\": true}}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenGenerateHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "generate-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"param\": {\"type\": \"parameter.v0\", \"value\": \"foo\", \"inputs\": {\"value\": {\"type\": \"string\", \"default\": {\"generate\": {\"minLength\": 8, \"extra\": true}}}}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenDaprMetadataHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "dapr-metadata-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"dapr\": {\"type\": \"dapr.v0\", \"dapr\": {\"application\": \"app\", \"appId\": \"id\", \"components\": [], \"foo\": true}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'foo'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenDaprComponentHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "dapr-component-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"comp\": {\"type\": \"dapr.component.v0\", \"daprComponent\": {\"type\": \"state.redis\", \"foo\": \"bar\"}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'foo'");
    }

    [Fact]
    public void LoadAndParseAspireManifest_ReturnsResource_WhenResourceTypeIsSupported()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "supportedType.json";
        fileSystem.AddFile(
            manifestFile, new("{\"resources\": {\"resource1\": {\"type\": \"container.v0\", \"image\": \"some-image\"}}}"));

        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        var result = service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        result.Should().HaveCount(1);
        result["resource1"].Should().NotBeOfType<UnsupportedResource>();
        result["resource1"].Should().BeOfType<ContainerResource>();
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenContainerV0HasBuildSection()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-with-build.json";
        fileSystem.AddFile(
            manifestFile,
            new("{\"resources\": {\"svc\": {\"type\": \"container.v0\", \"image\": \"img\", \"build\": {\"context\": \"./\", \"dockerfile\": \"Dockerfile\"}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not support property 'build'.");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenContainerV0HasUnknownProperty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-unknown.json";
        fileSystem.AddFile(
            manifestFile,
            new("{\"resources\": {\"svc\": {\"type\": \"container.v0\", \"image\": \"img\", \"foo\": \"bar\"}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'foo'");
    }

    [Theory]
    [InlineData("pg-endtoend.json", 22)]
    [InlineData("sqlserver-endtoend.json", 4)]
    [InlineData("starter-with-redis.json", 3)]
    [InlineData("project-no-binding.json", 1)]
    [InlineData("project-v1-deployment.json", 1)]
    [InlineData("container-v1-deployment.json", 1)]
    [InlineData("connectionstring-resource-expression.json", 5)]
    [InlineData("with-unsupported-resource.json", 6)]
    public async Task EndToEnd_ParsesSuccessfully(string manifestFile, int expectedCount)
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        await PerformEndToEndTests(manifestFile, expectedCount, serviceProvider, service, inputPopulator, valueSubstitutor);
    }

    [Fact]
    public async Task EndToEndWithManualEntry_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "starter-with-db.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var console = serviceProvider.GetRequiredService<IAnsiConsole>() as TestConsole;
        console.Profile.Capabilities.Interactive = true;
        EnterPasswordInput(console, "secret_password");

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        await PerformEndToEndTests(manifestFile, 8, serviceProvider, service, inputPopulator, valueSubstitutor);
    }

    [Fact]
    public async Task EndToEndShop_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "shop.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        var results = await PerformEndToEndTests(manifestFile, 12, serviceProvider, service, inputPopulator, valueSubstitutor);

        var shopResource = results["basketcache"] as ContainerResource;
        shopResource.Volumes.Should().HaveCount(1);
        shopResource.Volumes[0].Name.Should().Be("basketcache-data");
    }

    [Fact]
    public async Task ProjectV1Deployment_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "project-v1-deployment.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        var results = await PerformEndToEndTests(manifestFile, 1, serviceProvider, service, inputPopulator, valueSubstitutor);

        results["app"].Should().BeOfType<ProjectV1Resource>();
        var proj = results["app"] as ProjectV1Resource;
        proj!.Deployment.Should().NotBeNull();
        proj.Deployment!.Path.Should().Be("./redis.bicep");
        var projDeployment = proj.Deployment.Should().BeOfType<BicepV1Resource>().Subject;
        projDeployment.Scope!.ResourceGroup.Should().Be("rg-name");
        proj.Env.Should().ContainKey("ASPNETCORE_ENVIRONMENT");
    }

    [Fact]
    public async Task ContainerV1Deployment_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-v1-deployment.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        var results = await PerformEndToEndTests(manifestFile, 1, serviceProvider, service, inputPopulator, valueSubstitutor);

        results["cache"].Should().BeOfType<ContainerV1Resource>();
        var container = results["cache"] as ContainerV1Resource;
        container!.Deployment.Should().NotBeNull();
        container.Deployment!.Path.Should().Be("./redis.bicep");
        var containerDeployment = container.Deployment.Should().BeOfType<BicepV1Resource>().Subject;
        containerDeployment.Scope!.ResourceGroup.Should().Be("rg-name");
    }

    [Fact]
    public async Task ProjectV1Deployment_BicepV0_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "project-v1-deployment-v0.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        var results = await PerformEndToEndTests(manifestFile, 1, serviceProvider, service, inputPopulator, valueSubstitutor);

        results["app"].Should().BeOfType<ProjectV1Resource>();
        var proj = results["app"] as ProjectV1Resource;
        proj!.Deployment.Should().NotBeNull();
        proj.Deployment!.Path.Should().Be("./redis.bicep");
        proj.Deployment.Should().BeOfType<BicepResource>();
        proj.Deployment.Should().NotBeOfType<BicepV1Resource>();
    }

    [Fact]
    public void ProjectV1Deployment_InvalidType_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "project-v1-deployment-invalid-type.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(File.ReadAllText(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Deployment type 'azure.bicep.bad' must be 'azure.bicep.v0' or 'azure.bicep.v1'.*");
    }

    [Fact]
    public void ProjectV1Deployment_MissingType_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "project-v1-deployment-missing-type.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(File.ReadAllText(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Deployment missing required property 'type'.*");
    }

    [Fact]
    public async Task ContainerV1Deployment_BicepV0_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-v1-deployment-v0.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));

        var results = await PerformEndToEndTests(manifestFile, 1, serviceProvider, service, inputPopulator, valueSubstitutor);

        results["cache"].Should().BeOfType<ContainerV1Resource>();
        var container = results["cache"] as ContainerV1Resource;
        container!.Deployment.Should().NotBeNull();
        container.Deployment!.Path.Should().Be("./redis.bicep");
        container.Deployment.Should().BeOfType<BicepResource>();
        container.Deployment.Should().NotBeOfType<BicepV1Resource>();
    }

    [Fact]
    public void ContainerV1Deployment_InvalidType_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-v1-deployment-invalid-type.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(File.ReadAllText(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Deployment type 'azure.bicep.bad' must be 'azure.bicep.v0' or 'azure.bicep.v1'.*");
    }

    [Fact]
    public void ContainerV1Deployment_MissingType_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "container-v1-deployment-missing-type.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(File.ReadAllText(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        // Act
        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Deployment missing required property 'type'.*");
    }

    [Fact]
    public void LoadAndParseAspireManifest_Throws_WhenBicepScopeHasUnknownProperty()
    {
        var fileSystem = new MockFileSystem();
        var manifestFile = "bicep-scope-extra.json";
        fileSystem.AddFile(manifestFile,
            new("{\"resources\": {\"bicep\": {\"type\": \"azure.bicep.v1\", \"path\": \"./b.bicep\", \"scope\": {\"resourceGroup\": \"rg\", \"extra\": true}}}}"));
        var serviceProvider = CreateServiceProvider(fileSystem);
        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();

        Action act = () => service.LoadAndParseAspireManifest(manifestFile);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unexpected property 'extra'");
    }

    [Fact]
    public async Task EndToEndNodeJs_ParsesSuccessfully()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var manifestFile = "nodejs.json";
        var testData = Path.Combine(AppContext.BaseDirectory, "TestData", manifestFile);
        fileSystem.AddFile(manifestFile, new(await File.ReadAllTextAsync(testData)));
        var serviceProvider = CreateServiceProvider(fileSystem);

        var service = serviceProvider.GetRequiredService<IManifestFileParserService>();
        var inputPopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(PopulateInputsAction));
        var valueSubstitutor = serviceProvider.GetRequiredKeyedService<IAction>(nameof(SubstituteValuesAspireManifestAction));
        var cachePopulator = serviceProvider.GetRequiredKeyedService<IAction>(nameof(BuildAndPushContainersFromDockerfilesAction));
        var state = serviceProvider.GetRequiredService<AspirateState>();

        // Act
        state.SkipBuild = true;
        await PerformEndToEndTests(manifestFile, 1, serviceProvider, service, inputPopulator, valueSubstitutor);
        state.AspireComponentsToProcess = state.LoadedAspireManifestResources.Select(x=>x.Key).ToList();
        await cachePopulator.ExecuteAsync();
    }

    private static async Task<Dictionary<string, Resource>> PerformEndToEndTests(string manifestFile, int expectedCount, IServiceProvider serviceProvider, IManifestFileParserService service, IAction inputPopulator, IAction valueSubstitutor)
    {
        // Act
        var state = serviceProvider.GetRequiredService<AspirateState>();
        state.LoadedAspireManifestResources = service.LoadAndParseAspireManifest(manifestFile);
        await inputPopulator.ExecuteAsync();
        await valueSubstitutor.ExecuteAsync();
        var result = state.LoadedAspireManifestResources;

        // Assert
        result.Should().HaveCount(expectedCount);

        foreach (var container in result.Where(x => x.Value is ContainerResource))
        {
            var containerResource = container.Value as ContainerResource;
            containerResource.ConnectionString.Should().NotBeNullOrEmpty();
            containerResource.ConnectionString.Should().NotContain("{");
            containerResource.ConnectionString.Should().NotContain("}");

            foreach (var envVar in containerResource.Env)
            {
                envVar.Value.Should().NotContain("{");
                envVar.Value.Should().NotContain("}");
            }
        }

        foreach (var project in result.Where(x => x.Value is ProjectResource))
        {
            var containerResource = project.Value as ProjectResource;

            if (containerResource.Env is null)
            {
                continue;
            }

            foreach (var envVar in containerResource.Env)
            {
                envVar.Value.Should().NotContain("{");
                envVar.Value.Should().NotContain("}");
            }
        }

        return result;
    }

    private static IServiceProvider CreateServiceProvider(IFileSystem? fileSystem = null, IAnsiConsole? console = null)
    {
        console ??= new TestConsole();
        fileSystem ??= new FileSystem();
        var services = new ServiceCollection();
        services.RegisterAspirateEssential();
        services.RemoveAll<IAnsiConsole>();
        services.RemoveAll<IFileSystem>();
        services.AddSingleton(console);
        services.AddSingleton(fileSystem);
        services.AddSingleton<ISecretProvider, SecretProvider>();

        return services.BuildServiceProvider();
    }

    private static void EnterPasswordInput(TestConsole console, string password)
    {
        // first entry
        console.Input.PushTextWithEnter(password);
        console.Input.PushKey(ConsoleKey.Enter);

        // confirmation entry
        console.Input.PushTextWithEnter(password);
        console.Input.PushKey(ConsoleKey.Enter);
    }
}
