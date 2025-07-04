using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class SecretServiceTests : BaseServiceTests<ISecretService>
{
    private const string ValidState =
        """
        {
          "salt": "EgEu/M6c1XP/PCkG",
          "hash": "gSeKYq+cBB8Lx1Fw5iuImcUIONz99cQqt6052BjWLp4\u003d",
          "secrets": {
            "postgrescontainer": {
              "ConnectionString_Test": "EgEu/M6c1XP/PCkGUkJTJ9meX9wOz8mY0w0ca46KF3bVqqHah6QLTDwOyTHX"
            },
            "postgrescontainer2": {
              "ConnectionString_Test": "EgEu/M6c1XP/PCkGUkJTJ9meX9wOz8mY0w0ca46KF3bVqqHah6QLTDwOyTHX"
            }
          },
          "secretsVersion": 2
        }
        """;

    private const string InvalidVersionState =
        """
        {
          "salt": "EgEu/M6c1XP/PCkG",
          "hash": "gSeKYq+cBB8Lx1Fw5iuImcUIONz99cQqt6052BjWLp4\u003d",
          "secrets": {
            "postgrescontainer": {
              "ConnectionString_Test": "EgEu/M6c1XP/PCkGUkJTJ9meX9wOz8mY0w0ca46KF3bVqqHah6QLTDwOyTHX"
            }
          },
          "secretsVersion": 3
        }
        """;

    [Fact]
    public void LoadState_NotExistsInitialises_Success()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = null;
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);

        // Act
        state.SecretPassword = "test-password";

        service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = "test-password",
        });

        // Assert
        state.SecretState.Should().NotBeNull();
    }

    [Fact]
    public void  LoadState_ExistingState_Success()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");
        var state = CreateAspirateStateWithConnectionStrings();
        var serviceProvider = CreateServiceProvider(state, console);
        var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();
        var service = GetSystemUnderTest(serviceProvider);
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);

        // Act
        service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = false,
            DisableSecrets = false,
            SecretPassword = string.Empty,
        });

        // Assert
        secretProvider.State.Secrets.Count.Should().Be(2);
        secretProvider.State.Secrets.ElementAt(0).Value.Count.Should().Be(1);
        secretProvider.State.Secrets.ElementAt(1).Value.Count.Should().Be(1);
    }

    [Fact]
    public void LoadState_ExistingStateNonInteractive_Success()
    {
        // Arrange
        var console = new TestConsole();

        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "password_for_secrets");
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);

        var serviceProvider = CreateServiceProvider(state, console);
        var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();
        var service = GetSystemUnderTest(serviceProvider);

        // Act
        service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
        });

        // Assert
        secretProvider.State.Secrets.Count.Should().Be(2);
        secretProvider.State.Secrets.ElementAt(0).Value.Count.Should().Be(1);
        secretProvider.State.Secrets.ElementAt(1).Value.Count.Should().Be(1);
    }

    [Fact]
    public void LoadState_NonInteractiveNoPasswordThrows_Success()
    {
        // Arrange
        var console = new TestConsole();
        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true);
        var serviceProvider = CreateServiceProvider(state, console);
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var service = GetSystemUnderTest(serviceProvider);

        // Act
        var result = () => service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            CommandUnlocksSecrets = true,
        });;

        // Assert
        result.Should().Throw<ActionCausesExitException>();
    }

    [Fact]
    public void LoadState_NonInteractiveInvalidPasswordThrows_Success()
    {
        // Arrange
        var console = new TestConsole();
        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "invalid_password");
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);

        // Act
        var result = () => service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            CommandUnlocksSecrets = true,
        });

        // Assert
        result.Should().Throw<ActionCausesExitException>();
    }

    [Fact]
    public void LoadState_InvalidVersion_Warns()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");

        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(InvalidVersionState);
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);

        service.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = false,
            DisableSecrets = false,
            SecretPassword = string.Empty,
        });

        console.Output.Should().Contain("Secret state version mismatch");
    }

    [Fact]
    public void RotatePassword_RotatesSecrets()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");
        console.Input.PushTextWithEnter("new_secret_password");
        console.Input.PushTextWithEnter("new_secret_password");
        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);
        var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

        // Act
        service.RotatePassword(new SecretManagementOptions
        {
            State = state,
            NonInteractive = false,
            DisableSecrets = false,
            SecretPassword = string.Empty,
        });

        // Assert
        secretProvider.CheckPassword("new_secret_password").Should().BeTrue();
        secretProvider.SetPassword("new_secret_password");
        secretProvider.GetSecret("postgrescontainer", "ConnectionString_Test").Should().Be("some_secret_value");
    }

    [Fact]
    public void RotatePassword_RejectsWeakPassword()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");
        // weak password attempt
        console.Input.PushTextWithEnter("weak");
        console.Input.PushTextWithEnter("weak");
        // strong password attempt
        console.Input.PushTextWithEnter("StrongPass1!");
        console.Input.PushTextWithEnter("StrongPass1!");
        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);
        var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

        // Act
        service.RotatePassword(new SecretManagementOptions
        {
            State = state,
            NonInteractive = false,
            DisableSecrets = false,
            SecretPassword = string.Empty,
        });

        // Assert
        secretProvider.CheckPassword("StrongPass1!").Should().BeTrue();
        console.Output.Should().Contain("Password does not meet complexity requirements.");
    }

    [Fact]
    public void ClearSecrets_RemovesStateAndFile()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var statePath = "/state";
        fs.AddDirectory(statePath);
        var stateFile = fs.Path.Combine(statePath, AspirateLiterals.StateFileName);
        fs.AddFile(stateFile, "{}");

        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);

        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        service.ClearSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            SecretProvider = "file",
            StatePath = statePath,
            Force = true
        });

        state.SecretState.Should().BeNull();
        fs.FileExists(stateFile).Should().BeFalse();
    }

    [Fact]
    public void ClearSecrets_NonInteractiveNoForce_Throws()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var statePath = "/state";
        fs.AddDirectory(statePath);
        var stateFile = fs.Path.Combine(statePath, AspirateLiterals.StateFileName);
        fs.AddFile(stateFile, "{}");

        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);

        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        var act = () => service.ClearSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            SecretProvider = "file",
            StatePath = statePath
        });

        act.Should().Throw<ActionCausesExitException>();
        fs.FileExists(stateFile).Should().BeTrue();
    }

    [Fact]
    public void SaveSecrets_DetectsAdditionalVariables()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateStateWithAdditionalSecrets(nonInteractive: true, password: "test-password");

        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        service.SaveSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
        });

        secretProvider.State.Secrets["testcontainer"].Should().ContainKey(ProtectorType.JwtSecret.Value);
        secretProvider.State.Secrets["testcontainer"].Should().ContainKey(ProtectorType.RedisPassword.Value);
    }

    [Fact]
    public void SaveSecrets_AddsPrivateRegistrySecrets()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateState(nonInteractive: true, password: "test-password");
        state.WithPrivateRegistry = true;
        state.PrivateRegistryUrl = "https://registry.example.com";
        state.PrivateRegistryUsername = "user";
        state.PrivateRegistryPassword = "pass";
        state.PrivateRegistryEmail = "user@example.com";

        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        service.SaveSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
        });

        var resourceName = TemplateLiterals.ImagePullSecretType;
        secretProvider.State.Secrets.Should().ContainKey(resourceName);
        secretProvider.SetPassword(state.SecretPassword!);
        secretProvider.GetSecret(resourceName, "registryUrl").Should().Be(state.PrivateRegistryUrl);
        secretProvider.GetSecret(resourceName, "registryUsername").Should().Be(state.PrivateRegistryUsername);
        secretProvider.GetSecret(resourceName, "registryPassword").Should().Be("pass");
        secretProvider.GetSecret(resourceName, "registryEmail").Should().Be(state.PrivateRegistryEmail);
        state.PrivateRegistryPassword.Should().BeNull();
    }

    [Fact]
    public void Iterations_PersistAcrossRuns()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();

        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "test-password");
        var serviceProvider = CreateServiceProvider(state, console, fs, new SecretProvider(fs));
        var service = GetSystemUnderTest(serviceProvider);

        service.SaveSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
            Pbkdf2Iterations = 200_000
        });

        var newProvider = new SecretProvider(fs);
        var sp2 = CreateServiceProvider(state, console, fs, newProvider);
        var service2 = GetSystemUnderTest(sp2);

        service2.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
            CommandUnlocksSecrets = true
        });

        newProvider.Pbkdf2Iterations.Should().Be(200_000);
        newProvider.CheckPassword("test-password").Should().BeTrue();
    }

    [Fact]
    public async Task LoadStateAsync_NotExistsInitialises_Success()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = null;
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);

        state.SecretPassword = "test-password";

        await service.LoadSecretsAsync(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = "test-password",
            StatePath = "/"
        });

        state.SecretState.Should().NotBeNull();
    }

    [Fact]
    public async Task RotatePasswordAsync_RotatesSecrets()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");
        console.Input.PushTextWithEnter("new_secret_password");
        console.Input.PushTextWithEnter("new_secret_password");
        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console);
        var service = GetSystemUnderTest(serviceProvider);
        var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

        await service.RotatePasswordAsync(new SecretManagementOptions
        {
            State = state,
            NonInteractive = false,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            StatePath = "/"
        });

        secretProvider.CheckPassword("new_secret_password").Should().BeTrue();
        secretProvider.SetPassword("new_secret_password");
        secretProvider.GetSecret("postgrescontainer", "ConnectionString_Test").Should().Be("some_secret_value");
    }

    [Fact]
    public async Task SaveSecretsAsync_DetectsAdditionalVariables()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateStateWithAdditionalSecrets(nonInteractive: true, password: "test-password");
        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        await service.SaveSecretsAsync(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
            StatePath = "/"
        });

        secretProvider.State.Secrets["testcontainer"].Should().ContainKey(ProtectorType.JwtSecret.Value);
        secretProvider.State.Secrets["testcontainer"].Should().ContainKey(ProtectorType.RedisPassword.Value);
    }

    [Fact]
    public async Task ClearSecretsAsync_RemovesStateAndFile()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var statePath = "/state";
        fs.AddDirectory(statePath);
        var stateFile = fs.Path.Combine(statePath, AspirateLiterals.StateFileName);
        fs.AddFile(stateFile, "{}");

        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);

        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var service = GetSystemUnderTest(serviceProvider);

        await service.ClearSecretsAsync(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = string.Empty,
            SecretProvider = "file",
            StatePath = statePath,
            Force = true
        });

        state.SecretState.Should().BeNull();
        fs.FileExists(stateFile).Should().BeFalse();
    }
}
