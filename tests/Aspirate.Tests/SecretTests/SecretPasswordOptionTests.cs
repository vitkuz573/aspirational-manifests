using Aspirate.Cli;
using Aspirate.Commands.Options;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace Aspirate.Tests.SecretTests;

public class SecretPasswordOptionTests
{
    [Fact]
    public void ParsingCommand_ClearsSecretPasswordEnvironmentVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPIRATE_SECRET_PASSWORD", "test");
        var cli = new AspirateCli();

        // Act
        var result = cli.Parse(["generate"]);
        result.GetValueForOption(SecretPasswordOption.Instance);

        // Assert
        Environment.GetEnvironmentVariable("ASPIRATE_SECRET_PASSWORD").Should().BeNull();
    }
}
