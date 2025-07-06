using System;
using Xunit;
using Aspirate.Processors.Resources.Project;

namespace Aspirate.Tests.ProcessorTests;

public class RequiredPropertyValidationTests
{
    [Fact]
    public void DockerfileProcessor_MissingPath_Throws()
    {
        var fs = Substitute.For<IFileSystem>();
        var console = Substitute.For<IAnsiConsole>();
        var secretProvider = Substitute.For<ISecretProvider>();
        var comp = Substitute.For<IContainerCompositionService>();
        var details = Substitute.For<IContainerDetailsService>();
        var writer = Substitute.For<IManifestWriter>();

        var processor = new DockerfileProcessor(fs, console, secretProvider, comp, details, writer);

        var resource = new DockerfileResource { Context = "ctx" };
        var options = new CreateComposeEntryOptions
        {
            Resource = new("docker", resource),
            ComposeBuilds = true,
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'path'");
    }

    [Fact]
    public void DockerfileProcessor_MissingContext_Throws()
    {
        var processor = new DockerfileProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<ISecretProvider>(), Substitute.For<IContainerCompositionService>(), Substitute.For<IContainerDetailsService>(), Substitute.For<IManifestWriter>());

        var resource = new DockerfileResource { Path = "Dockerfile" };
        var options = new CreateComposeEntryOptions
        {
            Resource = new("docker", resource),
            ComposeBuilds = true,
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'context'");
    }

    [Fact]
    public void ContainerProcessor_MissingImage_Throws()
    {
        var processor = new ContainerProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<ISecretProvider>(), Substitute.For<IContainerCompositionService>(), Substitute.For<IContainerDetailsService>(), Substitute.For<IManifestWriter>());

        var resource = new ContainerResource { Image = null! };
        var options = new CreateComposeEntryOptions
        {
            Resource = new("cache", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'image'");
    }

    [Fact]
    public void ContainerV1Processor_MissingBuildContext_Throws()
    {
        var processor = new ContainerV1Processor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<ISecretProvider>(), Substitute.For<IContainerCompositionService>(), Substitute.For<IContainerDetailsService>(), Substitute.For<IManifestWriter>());

        var resource = new ContainerV1Resource
        {
            Build = new Build { Dockerfile = "Dockerfile", Context = null! },
        };

        var options = new CreateComposeEntryOptions
        {
            Resource = new("cache", resource),
            ComposeBuilds = true,
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required build context*");
    }

    [Fact]
    public void ExecutableProcessor_MissingCommand_Throws()
    {
        var processor = new ExecutableProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var resource = new ExecutableResource();

        var options = new CreateComposeEntryOptions
        {
            Resource = new("exec", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'command'");
    }

    [Fact]
    public void DaprProcessor_MissingMetadata_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var resource = new DaprResource();

        var options = new CreateComposeEntryOptions
        {
            Resource = new("dapr", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'metadata'");
    }

    [Fact]
    public void DaprProcessor_MissingApplication_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var resource = new DaprResource { Metadata = new() { AppId = "id", Components = ["comp"] } };

        var options = new CreateComposeEntryOptions
        {
            Resource = new("dapr", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'application'");
    }

    [Fact]
    public void DaprProcessor_MissingAppId_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var resource = new DaprResource { Metadata = new() { Application = "app", Components = ["comp"] } };

        var options = new CreateComposeEntryOptions
        {
            Resource = new("dapr", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'appId'");
    }

    [Fact]
    public void DaprProcessor_MissingComponents_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var resource = new DaprResource { Metadata = new() { Application = "app", AppId = "id" } };

        var options = new CreateComposeEntryOptions
        {
            Resource = new("dapr", resource),
        };

        var act = () => processor.CreateComposeEntry(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'components'");
    }

    [Fact]
    public void DaprProcessor_DeserializeMissingMetadata_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{}";

        var act = () =>
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var r = new Utf8JsonReader(bytes);
            r.Read();
            processor.Deserialize(ref r);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'metadata'");
    }

    [Fact]
    public void DaprProcessor_DeserializeMissingApplication_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"dapr\":{\"appId\":\"id\",\"components\":[\"comp\"]}}";

        var act = () =>
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var r = new Utf8JsonReader(bytes);
            r.Read();
            processor.Deserialize(ref r);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'application'");
    }

    [Fact]
    public void DaprProcessor_DeserializeMissingAppId_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"dapr\":{\"application\":\"app\",\"components\":[\"comp\"]}}";

        var act = () =>
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var r = new Utf8JsonReader(bytes);
            r.Read();
            processor.Deserialize(ref r);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'appId'");
    }

    [Fact]
    public void DaprProcessor_DeserializeMissingComponents_Throws()
    {
        var processor = new DaprProcessor(Substitute.For<IFileSystem>(), Substitute.For<IAnsiConsole>(), Substitute.For<IManifestWriter>());

        var json = "{\"dapr\":{\"application\":\"app\",\"appId\":\"id\"}}";

        var act = () =>
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var r = new Utf8JsonReader(bytes);
            r.Read();
            processor.Deserialize(ref r);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'components'");
    }

    [Fact]
    public void ProjectProcessor_DeserializeMissingPath_Throws()
    {
        var processor = new ProjectProcessor(
            Substitute.For<IFileSystem>(),
            Substitute.For<IAnsiConsole>(),
            Substitute.For<ISecretProvider>(),
            Substitute.For<IContainerCompositionService>(),
            Substitute.For<IContainerDetailsService>(),
            Substitute.For<IManifestWriter>());

        var json = "{}";

        var act = () =>
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var r = new Utf8JsonReader(bytes);
            r.Read();
            processor.Deserialize(ref r);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'path'");
    }
}
