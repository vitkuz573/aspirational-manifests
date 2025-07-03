namespace Aspirate.Secrets;

/// <summary>
/// Factory used to resolve the correct secret provider at runtime.
/// </summary>
public class SecretProviderFactory(IServiceProvider services)
{
    public ISecretProvider GetProvider(string? provider)
    {
        return services.GetRequiredService<SecretProvider>();
    }
}

