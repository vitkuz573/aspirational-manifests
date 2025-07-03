namespace Aspirate.Secrets;

public class Base64SecretProvider : ISecretProvider
{
    public SecretState? State { get; private set; }
    public int Pbkdf2Iterations { get; set; }

    public void AddResource(string resourceName)
    {
        State ??= new();
        if (!State.Secrets.ContainsKey(resourceName))
        {
            State.Secrets[resourceName] = [];
        }
    }

    public bool ResourceExists(string resourceName) => State?.Secrets.ContainsKey(resourceName) == true;

    public void RemoveResource(string resourceName) => State?.Secrets.Remove(resourceName);

    public bool SecretExists(string resourceName, string key) =>
        State?.Secrets.TryGetValue(resourceName, out var secrets) == true && secrets.ContainsKey(key);

    public void AddSecret(string resourceName, string key, string value)
    {
        AddResource(resourceName);
        State!.Secrets[resourceName][key] = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public void RemoveSecret(string resourceName, string key)
    {
        if (State?.Secrets.TryGetValue(resourceName, out var secrets) == true)
        {
            secrets.Remove(key);
        }
    }

    public void SetState(AspirateState state)
    {
        state.SecretState = State;
    }

    public void LoadState(AspirateState state)
    {
        State = state.SecretState ?? new();
    }

    public void RemoveState(AspirateState state)
    {
        State = null;
        state.SecretState = null;
    }

    public bool SecretStateExists(AspirateState state) => state.SecretState != null;

    public string? GetSecret(string resourceName, string key)
    {
        if (State?.Secrets.TryGetValue(resourceName, out var secrets) == true &&
            secrets.TryGetValue(key, out var encoded))
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }

        return null;
    }

    public void SetPassword(string password) { }
    public bool CheckPassword(string password) => true;
    public void RotatePassword(string newPassword) { }
    public void UpgradeEncryption() { }
    public void ClearPassword() { }
}
