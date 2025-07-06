namespace Aspirate.Shared.Literals;

public static class AspirateSecretLiterals
{
    /// <summary>
    /// Identifier used for the default file backed secrets manager.
    /// </summary>
    public const string FileSecretsManager = "file";

    /// <summary>
    /// Identifier used for secrets stored as environment variables.
    /// </summary>
    public const string EnvironmentSecretsManager = "env";

    /// <summary>
    /// Alternative identifier used for environment variable secrets manager.
    /// </summary>
    public const string EnvironmentSecretsManagerLong = "environment";

    /// <summary>
    /// Identifier used for the password protected secrets manager.
    /// </summary>
    public const string PasswordSecretsManager = "password";

    /// <summary>
    /// Identifier used for the base64 encoded secrets manager.
    /// </summary>
    public const string Base64SecretsManager = "base64";
}
