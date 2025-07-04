using Aspirate.Cli;
using Aspirate.Commands.Options;
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
        var parser = new Parser(cli);

        // Act
        var result = parser.Parse(["generate"]);
        result.GetValueForOption(SecretPasswordOption.Instance);

        // Assert
        Environment.GetEnvironmentVariable("ASPIRATE_SECRET_PASSWORD").Should().BeNull();
    }
}
