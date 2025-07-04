using Aspirate.Commands.Commands.VerifySecrets;
using Xunit;

namespace Aspirate.Tests.CommandTests;

public class VerifySecretsCommandHandlerTests : AspirateTestBase
{
    private const string ValidState =
        """
        {
          "salt": "EgEu/M6c1XP/PCkG",
          "hash": "gSeKYq+cBB8Lx1Fw5iuImcUIONz99cQqt6052BjWLp4\u003d",
          "secrets": {
            "postgrescontainer": {
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
          "secretsVersion": 1
        }
        """;

    [Fact]
    public async Task HandleAsync_VerifiesPassword()
    {
        var console = new TestConsole();
        var fs = new MockFileSystem();
        var secretProvider = new SecretProvider(fs);

        var state = CreateAspirateStateWithConnectionStrings(nonInteractive: true, password: "password_for_secrets");
        state.SecretState = JsonSerializer.Deserialize<SecretState>(ValidState);
        var sp = CreateServiceProvider(state, console, fs, secretProvider);

        var handler = new VerifySecretsCommandHandler(sp);

        await handler.HandleAsync(new VerifySecretsOptions
        {
            NonInteractive = true,
            SecretPassword = "password_for_secrets"
        });

        console.Output.Should().Contain("Secrets verified successfully");
    }

    [Fact]
    public async Task HandleAsync_WarnsOnOldVersion()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("password_for_secrets");

        var state = CreateAspirateStateWithConnectionStrings();
        state.SecretState = JsonSerializer.Deserialize<SecretState>(InvalidVersionState);
        var sp = CreateServiceProvider(state, console);

        var handler = new VerifySecretsCommandHandler(sp);

        await handler.HandleAsync(new VerifySecretsOptions());

        console.Output.Should().Contain("Secret state version mismatch");
    }
}
