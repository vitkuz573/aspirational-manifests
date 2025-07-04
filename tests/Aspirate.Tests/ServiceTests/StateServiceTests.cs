using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Linq;
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

    [Fact]
    public async Task SaveState_SetsSecurePermissions()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>(), "/");
        var console = new TestConsole();
        var secretProvider = new SecretProvider(fs);
        var sut = new StateService(fs, console, secretProvider);

        var statePath = "/state";
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

        await sut.SaveState(options);

        var stateFile = fs.Path.Combine(statePath, AspirateLiterals.StateFileName);

        if (OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var fileInfo = fs.FileInfo.New(stateFile);
            var acl = fileInfo.GetAccessControl();
            var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>();
            var currentUser = WindowsIdentity.GetCurrent().User;
            rules.Should().Contain(r => r.IdentityReference.Equals(currentUser) &&
                                       r.FileSystemRights.HasFlag(FileSystemRights.Read) &&
                                       r.FileSystemRights.HasFlag(FileSystemRights.Write));
#pragma warning restore CA1416
        }
        else
        {
            var mode = fs.File.GetUnixFileMode(stateFile);
            mode.Should().Be(UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
