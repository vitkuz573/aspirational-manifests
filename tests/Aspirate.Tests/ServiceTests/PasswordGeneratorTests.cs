using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class PasswordGeneratorTests : BaseServiceTests<IPasswordGenerator>
{
    [Fact]
    public void Generate_RespectsMinimumCounts()
    {
        var state = CreateAspirateState();
        var serviceProvider = CreateServiceProvider(state);
        var generator = GetSystemUnderTest(serviceProvider);

        var options = new Generate
        {
            MinLength = 10,
            MinLower = 1,
            MinUpper = 1,
            MinNumeric = 1,
            MinSpecial = 1
        };

        var password = generator.Generate(options);

        password.Should().HaveLength(10);
        password.Count(char.IsLower).Should().BeGreaterOrEqualTo(1);
        password.Count(char.IsUpper).Should().BeGreaterOrEqualTo(1);
        password.Count(char.IsDigit).Should().BeGreaterOrEqualTo(1);
        password.Count(c => !char.IsLetterOrDigit(c)).Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void Validate_ReturnsFalse_ForInvalidPassword()
    {
        var state = CreateAspirateState();
        var serviceProvider = CreateServiceProvider(state);
        var generator = GetSystemUnderTest(serviceProvider);

        var options = new Generate
        {
            MinLength = 5,
            MinUpper = 1
        };

        var result = generator.Validate("abcde", options);

        result.Should().BeFalse();
    }
}
