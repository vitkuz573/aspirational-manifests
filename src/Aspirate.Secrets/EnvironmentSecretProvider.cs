namespace Aspirate.Secrets;

public class EnvironmentSecretProvider : ISecretProvider
{
    public SecretState? State { get; private set; }
    public int Pbkdf2Iterations { get; set; }

    private static string BuildName(string resourceName, string key) => $"{resourceName}_{key}".ToUpperInvariant();

    public void AddResource(string resourceName) { }
    public bool ResourceExists(string resourceName) => true;
    public void RemoveResource(string resourceName) { }

    public bool SecretExists(string resourceName, string key) =>
        Environment.GetEnvironmentVariable(BuildName(resourceName, key)) != null;

    public void AddSecret(string resourceName, string key, string value) =>
        Environment.SetEnvironmentVariable(BuildName(resourceName, key), value);

    public void RemoveSecret(string resourceName, string key) =>
        Environment.SetEnvironmentVariable(BuildName(resourceName, key), null);

    public void SetState(AspirateState state) { }
    public void LoadState(AspirateState state) { }
    public void RemoveState(AspirateState state) { }
    public bool SecretStateExists(AspirateState state) => true;

    public string? GetSecret(string resourceName, string key) =>
        Environment.GetEnvironmentVariable(BuildName(resourceName, key));

    public void SetPassword(string password) { }
    public bool CheckPassword(string password) => true;
    public void RotatePassword(string newPassword) { }
    public void UpgradeEncryption() { }
    public void ClearPassword() { }
}
