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
        fs.AddDirectory(fs.Path.GetTempPath());

        var shellExecutionService = Substitute.For<IShellExecutionService>();
        var console = Substitute.For<IAnsiConsole>();
        var sut = new KustomizeService(fs, shellExecutionService, console);

        var state = CreateAspirateStateWithConnectionStrings();
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
}
