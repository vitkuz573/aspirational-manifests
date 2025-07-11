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
}
