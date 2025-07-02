using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class StateServiceTests : BaseServiceTests<IStateService>
{
    [Fact]
    public async Task SaveState_WritesFile_ToStatePath()
    {
        // Arrange
        var fs = new MockFileSystem();
        var console = new TestConsole();
        var secretProvider = new SecretProvider(fs);
        var sut = new StateService(fs, console, secretProvider);

        var statePath = fs.Path.Combine("/custom", "state");
        fs.AddDirectory(statePath);

        var state = CreateAspirateState();

        var options = new StateManagementOptions
        {
            State = state,
            DisableState = false,
            NonInteractive = true,
            RequiresState = false,
            StatePath = statePath,
        };

        // Act
        await sut.SaveState(options);

        // Assert
        var stateFile = fs.Path.Combine(statePath, AspirateLiterals.StateFileName);
        fs.FileExists(stateFile).Should().BeTrue();
    }

    [Fact]
    public async Task RestoreState_ReadsFile_FromStatePath()
    {
        // Arrange
        var fs = new MockFileSystem();
        var console = new TestConsole();
        var secretProvider = new SecretProvider(fs);
        var sut = new StateService(fs, console, secretProvider);

        var statePath = fs.Path.Combine("/custom", "state");
        fs.AddDirectory(statePath);

        var initialState = CreateAspirateState();
        var saveOptions = new StateManagementOptions
        {
            State = initialState,
            DisableState = false,
            NonInteractive = true,
            RequiresState = false,
            StatePath = statePath,
        };
        await sut.SaveState(saveOptions);

        var newState = CreateAspirateState(projectPath: null);
        var restoreOptions = new StateManagementOptions
        {
            State = newState,
            DisableState = false,
            NonInteractive = true,
            RequiresState = false,
            StatePath = statePath,
        };

        // Act
        await sut.RestoreState(restoreOptions);

        // Assert
        newState.ProjectPath.Should().Be(initialState.ProjectPath);
    }
}
