using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class KustomizeServiceTests : AspirateTestBase
{
    [Fact]
    public async Task WriteSecretsOutToTempFiles_AndCleanupSecretEnvFiles_WorkTogether()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.AddDirectory("/input");
        fs.AddDirectory("/input/postgrescontainer");
        fs.AddDirectory("/input/postgrescontainer2");

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateStateWithConnectionStrings();
        state.InputPath = "/input";
        state.SecretState = new SecretState
        {
            Secrets = new()
            {
                ["postgrescontainer"] = new()
                {
                    ["ConnectionString_Test"] = "dummy"
                }
            }
        };
        var secretProvider = new SecretProvider(fs);
        secretProvider.LoadState(state);

        var files = new List<string>();

        // Act
        await sut.WriteSecretsOutToTempFiles(state, files, secretProvider);

        // Assert - files created
        files.Should().NotBeEmpty();
        files.Should().AllSatisfy(file => fs.FileExists(file).Should().BeTrue());

        sut.CleanupSecretEnvFiles(state.DisableSecrets, files);

        // Assert - files removed
        files.Should().AllSatisfy(file => fs.FileExists(file).Should().BeFalse());
    }

    [Fact]
    public async Task WriteImagePullSecretToTempFile_CreatesFile()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(fs.Path.GetTempPath());

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateState();
        state.WithPrivateRegistry = true;

        var secretProvider = new SecretProvider(fs)
        {
            State = new SecretState()
        };
        secretProvider.SetPassword("pwd");
        secretProvider.AddResource(TemplateLiterals.ImagePullSecretType);
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryUrl", "https://registry.example.com");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryUsername", "user");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryPassword", "pass");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryEmail", "user@example.com");
        secretProvider.SetState(state);

        var result = await sut.WriteImagePullSecretToTempFile(state, secretProvider);

        result.Should().NotBeNull();
        fs.FileExists(result!).Should().BeTrue();
    }

    [Fact]
    public async Task RenderManifestUsingKustomize_UsesOverlayPath()
    {
        var fs = new MockFileSystem();
        var shellExecutionService = Substitute.For<IShellExecutionService>();
        shellExecutionService.ExecuteCommand(Arg.Any<ShellCommandOptions>())
            .Returns(Task.FromResult(new ShellCommandResult(true, "output", string.Empty, 0)));

        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var overlay = "/base/overlays/dev";
        var result = await sut.RenderManifestUsingKustomize("/base", overlay);

        await shellExecutionService.Received(1)
            .ExecuteCommand(Arg.Is<ShellCommandOptions>(o =>
                o.ArgumentsBuilder!.RenderArguments().Contains(overlay)));

        result.Should().Be("output");
    }

    [Fact]
    public async Task WriteSecretsOutToTempFiles_CreatesEmptyFilesInOverlay()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory("/base/overlays/dev/postgrescontainer");

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateState();
        state.InputPath = "/base";
        state.OverlayPath = "/base/overlays/dev";
        state.SecretState = new SecretState();

        var secretProvider = new SecretProvider(fs);
        secretProvider.LoadState(state);

        var files = new List<string>();

        await sut.WriteSecretsOutToTempFiles(state, files, secretProvider);

        var expected = "/base/overlays/dev/postgrescontainer/.postgrescontainer.secrets";
        fs.FileExists(expected).Should().BeTrue();
        fs.File.ReadAllText(expected).Should().BeEmpty();
    }

    [Fact]
    public async Task WriteSecretsOutToTempFiles_WritesSecretsToOverlay()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory("/base/overlays/dev/postgrescontainer");

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateStateWithConnectionStrings();
        state.InputPath = "/base";
        state.OverlayPath = "/base/overlays/dev";
        state.SecretState = new SecretState
        {
            Secrets = new()
            {
                ["postgrescontainer"] = new()
                {
                    ["ConnectionString_Test"] = "dummy"
                }
            }
        };

        var secretProvider = new SecretProvider(fs);
        secretProvider.LoadState(state);

        var files = new List<string>();

        await sut.WriteSecretsOutToTempFiles(state, files, secretProvider);

        var expected = "/base/overlays/dev/postgrescontainer/.postgrescontainer.secrets";
        var contents = fs.File.ReadAllText(expected);
        contents.Should().Contain("ConnectionString_Test=dummy");
    }

    [Fact]
    public async Task WriteSecretsOutToTempFiles_CreatesEmptyFilesInNestedOverlay()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory("/overlays/prod");
        fs.AddDirectory("/overlays/api/prod");
        fs.AddDirectory("/overlays/panel/prod");

        fs.AddFile("/overlays/prod/kustomization.yaml", new("resources:\n- ../api/prod\n- ../panel/prod\n"));
        fs.AddFile("/overlays/api/prod/kustomization.yaml", new("secretGenerator:\n- name: api-secrets\n  envs:\n  - .api.secrets\n"));
        fs.AddFile("/overlays/panel/prod/kustomization.yaml", new("secretGenerator:\n- name: panel-secrets\n  envs:\n  - .panel.secrets\n"));

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateState();
        state.InputPath = "/overlays";
        state.OverlayPath = "/overlays/prod";
        state.SecretState = new SecretState();

        var secretProvider = new SecretProvider(fs);
        secretProvider.LoadState(state);

        var files = new List<string>();

        await sut.WriteSecretsOutToTempFiles(state, files, secretProvider);

        fs.FileExists("/overlays/api/prod/.api.secrets").Should().BeTrue();
        fs.FileExists("/overlays/panel/prod/.panel.secrets").Should().BeTrue();
        fs.File.ReadAllText("/overlays/api/prod/.api.secrets").Should().BeEmpty();
        fs.File.ReadAllText("/overlays/panel/prod/.panel.secrets").Should().BeEmpty();
    }

    [Fact]
    public async Task WriteSecretsOutToTempFiles_WritesSecretsInNestedOverlay()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory("/overlays/prod");
        fs.AddDirectory("/overlays/api/prod");
        fs.AddDirectory("/overlays/panel/prod");

        fs.AddFile("/overlays/prod/kustomization.yaml", new("resources:\n- ../api/prod\n- ../panel/prod\n"));
        fs.AddFile("/overlays/api/prod/kustomization.yaml", new("secretGenerator:\n- name: api-secrets\n  envs:\n  - .api.secrets\n"));
        fs.AddFile("/overlays/panel/prod/kustomization.yaml", new("secretGenerator:\n- name: panel-secrets\n  envs:\n  - .panel.secrets\n"));

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var manifestWriter = new ManifestWriter(fs);
        var sut = new KustomizeService(fs, shellExecutionService, console, manifestWriter);

        var state = CreateAspirateState();
        state.InputPath = "/overlays";
        state.OverlayPath = "/overlays/prod";
        state.SecretState = new SecretState
        {
            Secrets = new()
            {
                ["api"] = new()
                {
                    ["token"] = "1234"
                },
                ["panel"] = new()
                {
                    ["pwd"] = "abcd"
                }
            }
        };

        var secretProvider = new SecretProvider(fs);
        secretProvider.LoadState(state);

        var files = new List<string>();

        await sut.WriteSecretsOutToTempFiles(state, files, secretProvider);

        fs.FileExists("/overlays/api/prod/.api.secrets").Should().BeTrue();
        fs.FileExists("/overlays/panel/prod/.panel.secrets").Should().BeTrue();

        fs.File.ReadAllText("/overlays/api/prod/.api.secrets").Should().Contain("token=1234");
        fs.File.ReadAllText("/overlays/panel/prod/.panel.secrets").Should().Contain("pwd=abcd");
    }
}
