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
            null or "" or Aspirate.Shared.Literals.AspirateSecretLiterals.FileSecretsManager or Aspirate.Shared.Literals.AspirateSecretLiterals.PasswordSecretsManager =>
                services.GetRequiredService<SecretProvider>(),
            Aspirate.Shared.Literals.AspirateSecretLiterals.EnvironmentSecretsManager or Aspirate.Shared.Literals.AspirateSecretLiterals.EnvironmentSecretsManagerLong =>
                services.GetRequiredService<EnvironmentSecretProvider>(),
            Aspirate.Shared.Literals.AspirateSecretLiterals.Base64SecretsManager =>
                services.GetRequiredService<Base64SecretProvider>(),
            _ => throw new InvalidOperationException($"Unknown secret provider '{provider}'.")
        };
    }
}

