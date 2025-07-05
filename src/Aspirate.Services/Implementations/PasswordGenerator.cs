namespace Aspirate.Services.Implementations;

public class PasswordGenerator : IPasswordGenerator
{
    private const string LowerCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericCharacters = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()-_=+[]";

    public string Generate(Generate options)
    {
        var pool = new List<char>();
        if (options.Lower)
        {
            pool.AddRange(LowerCharacters);
        }
        if (options.Upper)
        {
            pool.AddRange(UpperCharacters);
        }
        if (options.Numeric)
        {
            pool.AddRange(NumericCharacters);
        }
        if (options.Special)
        {
            pool.AddRange(SpecialCharacters);
        }

        if (pool.Count == 0)
        {
            throw new InvalidOperationException("No characters available to generate password.");
        }

        var builder = new StringBuilder(options.MinLength);

        AppendRandom(builder, LowerCharacters, options.MinLower);
        AppendRandom(builder, UpperCharacters, options.MinUpper);
        AppendRandom(builder, NumericCharacters, options.MinNumeric);
        AppendRandom(builder, SpecialCharacters, options.MinSpecial);

        for (var i = builder.Length; i < options.MinLength; i++)
        {
            builder.Append(pool[RandomNumberGenerator.GetInt32(pool.Count)]);
        }

        return Shuffle(builder.ToString());
    }

    public bool Validate(string value, Generate options)
    {
        if (value.Length < options.MinLength)
        {
            return false;
        }

        if (options.Lower && value.Count(char.IsLower) < options.MinLower)
        {
            return false;
        }

        if (!options.Lower && value.Any(char.IsLower))
        {
            return false;
        }

        if (options.Upper && value.Count(char.IsUpper) < options.MinUpper)
        {
            return false;
        }

        if (!options.Upper && value.Any(char.IsUpper))
        {
            return false;
        }

        if (options.Numeric && value.Count(char.IsDigit) < options.MinNumeric)
        {
            return false;
        }

        if (!options.Numeric && value.Any(char.IsDigit))
        {
            return false;
        }

        var specialCount = value.Count(c => !char.IsLetterOrDigit(c));
        if (options.Special && specialCount < options.MinSpecial)
        {
            return false;
        }

        if (!options.Special && specialCount > 0)
        {
            return false;
        }

        return true;
    }

    private static void AppendRandom(StringBuilder builder, string chars, int count)
    {
        for (var i = 0; i < count; i++)
        {
            builder.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
        }
    }

    private static string Shuffle(string value)
    {
        var chars = value.ToCharArray();
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
