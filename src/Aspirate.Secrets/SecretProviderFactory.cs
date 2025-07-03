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
            null or "" or "file" or "password" => services.GetRequiredService<SecretProvider>(),
            "env" or "environment" => services.GetRequiredService<EnvironmentSecretProvider>(),
            "base64" => services.GetRequiredService<Base64SecretProvider>(),
            _ => throw new InvalidOperationException($"Unknown secret provider '{provider}'.")
        };
    }
}

