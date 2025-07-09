using Aspirate.Commands.Commands.ListSecrets;
using Xunit;

namespace Aspirate.Tests.CommandTests;

public class ListSecretsCommandHandlerTests : AspirateTestBase
{
    private const string ValidState =
        """
        {
          "salt": "EgEu/M6c1XP/PCkG",
          "hash": "gSeKYq+cBB8Lx1Fw5iuImcUIONz99cQqt6052BjWLp4\u003d",
          "secrets": {
            "postgrescontainer": {
              "ConnectionString_Test": "pS2TKkcv5CsdOmBrZoNrxebeFYacDs2LM+QDGtYvHLIJPJ6JO6y2XUk4GULsVGYS"
            },
            "postgrescontainer2": {
              "ConnectionString_Test": "pS2TKkcv5CsdOmBrZoNrxebeFYacDs2LM+QDGtYvHLIJPJ6JO6y2XUk4GULsVGYS"
            }
          },
          "secretsVersion": 2,
          "pbkdf2Iterations": 1000000
        }
        """;

    [Fact]
    public async Task HandleAsync_DisplaysSecretsPerResource()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "password_for_secrets");
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var secretService = serviceProvider.GetRequiredService<ISecretService>();

        secretService.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
            SecretProvider = AspirateSecretLiterals.FileSecretsManager,
            CommandUnlocksSecrets = true,
        });

        var handler = new ListSecretsCommandHandler(serviceProvider);

        await handler.HandleAsync(new ListSecretsOptions
        {
            NonInteractive = true,
            SecretPassword = "password_for_secrets"
        });

        console.Output.Should().Contain("postgrescontainer");
        console.Output.Should().Contain("ConnectionString_Test");
        console.Output.Should().Contain(new MaskedValue("some_secret_value").ToString());
    }

    [Fact]
    public async Task HandleAsync_FilterByResourceName()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "password_for_secrets");
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var serviceProvider = CreateServiceProvider(state, console, fs, secretProvider);
        var secretService = serviceProvider.GetRequiredService<ISecretService>();

        secretService.LoadSecrets(new SecretManagementOptions
        {
            State = state,
            NonInteractive = true,
            DisableSecrets = false,
            SecretPassword = state.SecretPassword,
            SecretProvider = AspirateSecretLiterals.FileSecretsManager,
            CommandUnlocksSecrets = true,
        });

        var handler = new ListSecretsCommandHandler(serviceProvider);

        await handler.HandleAsync(new ListSecretsOptions
        {
            NonInteractive = true,
            SecretPassword = "password_for_secrets",
            Resource = "postgrescontainer"
        });

        console.Output.Should().Contain("postgrescontainer");
        console.Output.Should().NotContain("postgrescontainer2");
    }
}
