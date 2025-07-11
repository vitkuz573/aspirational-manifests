using System;
using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ActionsTests.Secrets;

public class PopulateInputsActionTests : BaseActionTests<PopulateInputsAction>
{
    [Fact]
    public async Task ExecuteAsync_InInteractiveMode_ReturnsCorrectResult()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        EnterPasswordInput(console, "secret_password"); // postgrescontainer
        EnterPasswordInput(console, "other_secret_password"); // postgresContainer2
        var state = CreateAspirateStateWithInputs();
        var serviceProvider = CreateServiceProvider(state, console);
        var action = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Should().BeTrue();

        var postgresParams = state.LoadedAspireManifestResources["postgresparams1"] as ParameterResource;
        var postgres2Params = state.LoadedAspireManifestResources["postgresparams2"] as ParameterResource;

        postgresParams.Value.Should().Be("secret_password");
        postgres2Params.Value.Should().Be("other_secret_password");
    }

    [Fact]
    public async Task PopulateInputs_SecretFlagTrue_StoresValueInSecretProvider()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        var fileSystem = new MockFileSystem();
        var secretProvider = new SecretProvider(fileSystem);
        var state = CreateAspirateState(nonInteractive: true);
        state.SecretState = new SecretState();
        var parameter = new ParameterResource
        {
            Name = "paramsecret",
            Type = "string",
            Inputs = new Dictionary<string, ParameterInput>
            {
                ["value"] = new()
                {
                    Type = "string",
                    Secret = true,
                    Default = new ParameterDefault
                    {
                        Generate = new Generate { MinLength = 8 }
                    }
                }
            }
        };
        state.LoadedAspireManifestResources = new Dictionary<string, Resource>
        {
            ["paramsecret"] = parameter
        };
        state.AspireComponentsToProcess = ["paramsecret"];

        secretProvider.LoadState(state);
        secretProvider.SetPassword("pwd");

        var serviceProvider = CreateServiceProvider(state, console, fileSystem, secretProvider);
        var action = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
        secretProvider.State.Secrets.Should().ContainKey("paramsecret");
        secretProvider.GetSecret("paramsecret", "value").Should().Be(parameter.Value);
    }

    [Fact]
    public async Task PopulateInputs_SecretFlagFalse_DoesNotStoreValue()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        var fileSystem = new MockFileSystem();
        var secretProvider = new SecretProvider(fileSystem);
        var state = CreateAspirateState(nonInteractive: true);
        state.SecretState = new SecretState();
        var parameter = new ParameterResource
        {
            Name = "plaintext",
            Type = "string",
            Inputs = new Dictionary<string, ParameterInput>
            {
                ["value"] = new()
                {
                    Type = "string",
                    Secret = false,
                    Default = new ParameterDefault
                    {
                        Generate = new Generate { MinLength = 8 }
                    }
                }
            }
        };
        state.LoadedAspireManifestResources = new Dictionary<string, Resource>
        {
            ["plaintext"] = parameter
        };
        state.AspireComponentsToProcess = ["plaintext"];

        secretProvider.LoadState(state);
        secretProvider.SetPassword("pwd");

        var serviceProvider = CreateServiceProvider(state, console, fileSystem, secretProvider);
        var action = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
        secretProvider.State.Secrets.Should().NotContainKey("plaintext");
        parameter.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_NonInteractiveMode_ReturnsCorrectResult()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        var state = CreateAspirateStateWithInputs(nonInteractive: true, generatedInputs: true);
        var serviceProvider = CreateServiceProvider(state, console);
        var action = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Should().BeTrue();

        var postgresParams = state.LoadedAspireManifestResources["postgresparams1"] as ParameterResource;
        var postgres2Params = state.LoadedAspireManifestResources["postgresparams2"] as ParameterResource;

        postgresParams.Value.Should().HaveLength(22);
        postgres2Params.Value.Should().HaveLength(22);
        postgresParams.Value.Should().NotBe(postgres2Params.Value);
        postgresParams.Value.Count(char.IsLower).Should().BeGreaterThanOrEqualTo(1);
        postgresParams.Value.Count(char.IsUpper).Should().BeGreaterThanOrEqualTo(1);
        postgresParams.Value.Count(char.IsDigit).Should().BeGreaterThanOrEqualTo(1);
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

    [Fact]
    public void ParameterInput_InvalidType_Throws()
    {
        var input = new ParameterInput();

        var act = () => input.Type = "number";

        act.Should().Throw<InvalidOperationException>();
    }
}
