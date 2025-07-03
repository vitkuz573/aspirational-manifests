namespace Aspirate.Secrets;

/// <summary>
/// Secret provider that stores secrets in Azure Key Vault.
/// </summary>
public class AzureKeyVaultSecretProvider(IConfiguration configuration) : ISecretProvider
{
    private readonly SecretClient _client = new(
        new Uri(configuration["ASPIRATE_KEYVAULT_URI"] ?? throw new InvalidOperationException("ASPIRATE_KEYVAULT_URI not set")),
        new DefaultAzureCredential());

    public SecretState? State { get; set; } = new();
    public int Pbkdf2Iterations { get; set; }

    public void AddResource(string resourceName) { }

    public bool ResourceExists(string resourceName) => true;

    public void RemoveResource(string resourceName) { }

    public bool SecretExists(string resourceName, string key)
    {
        try
        {
            _ = _client.GetSecret($"{resourceName}-{key}");
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public void AddSecret(string resourceName, string key, string value) =>
        _client.SetSecret($"{resourceName}-{key}", value);

    public void RemoveSecret(string resourceName, string key) =>
        _client.StartDeleteSecret($"{resourceName}-{key}");

    public void SetState(AspirateState state) { }

    public void LoadState(AspirateState state) { }

    public void RemoveState(AspirateState state) { }

    public bool SecretStateExists(AspirateState state) => true;

    public string? GetSecret(string resourceName, string key)
    {
        try
        {
            return _client.GetSecret($"{resourceName}-{key}").Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public void SetPassword(string password) { }

    public bool CheckPassword(string password) => true;

    public void RotatePassword(string newPassword) { }

    public void ClearPassword() { }
}

