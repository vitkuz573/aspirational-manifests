namespace Aspirate.Secrets;

/// <summary>
/// Factory used to resolve the correct secret provider at runtime.
/// </summary>
public class SecretProviderFactory(IServiceProvider services)
{
    public ISecretProvider GetProvider(string? provider)
    {
        return (provider?.ToLowerInvariant()) switch
        {
            null or "" or AspirateSecretLiterals.FileSecretsManager or AspirateSecretLiterals.PasswordSecretsManager =>
                services.GetRequiredService<SecretProvider>(),
            AspirateSecretLiterals.EnvironmentSecretsManager or AspirateSecretLiterals.EnvironmentSecretsManagerLong =>
                services.GetRequiredService<EnvironmentSecretProvider>(),
            AspirateSecretLiterals.Base64SecretsManager =>
                services.GetRequiredService<Base64SecretProvider>(),
            _ => throw new InvalidOperationException($"Unknown secret provider '{provider}'.")
        };
    }
}

