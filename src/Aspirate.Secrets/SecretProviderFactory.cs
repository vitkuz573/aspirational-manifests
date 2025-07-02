namespace Aspirate.Secrets;

/// <summary>
/// Factory used to resolve the correct secret provider at runtime.
/// </summary>
public class SecretProviderFactory(IServiceProvider services)
{
    public ISecretProvider GetProvider(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "keyvault" => services.GetRequiredService<AzureKeyVaultSecretProvider>(),
            _ => services.GetRequiredService<SecretProvider>(),
        };
    }
}

