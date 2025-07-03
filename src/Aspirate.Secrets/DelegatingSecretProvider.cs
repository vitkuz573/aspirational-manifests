namespace Aspirate.Secrets;

/// <summary>
/// Delegates secret operations to the provider selected in <see cref="AspirateState"/>.
/// </summary>
public class DelegatingSecretProvider(SecretProviderFactory factory, AspirateState state) : ISecretProvider
{
    private ISecretProvider Resolve() =>
        factory.GetProvider(state.SecretProvider ?? Environment.GetEnvironmentVariable("ASPIRATE_SECRET_PROVIDER"));

    public SecretState? State => Resolve().State;

    public void AddResource(string resourceName) => Resolve().AddResource(resourceName);
    public bool ResourceExists(string resourceName) => Resolve().ResourceExists(resourceName);
    public void RemoveResource(string resourceName) => Resolve().RemoveResource(resourceName);
    public bool SecretExists(string resourceName, string key) => Resolve().SecretExists(resourceName, key);
    public void AddSecret(string resourceName, string key, string value) => Resolve().AddSecret(resourceName, key, value);
    public void RemoveSecret(string resourceName, string key) => Resolve().RemoveSecret(resourceName, key);
    public void SetState(AspirateState stateArg) => Resolve().SetState(stateArg);
    public void LoadState(AspirateState stateArg) => Resolve().LoadState(stateArg);
    public void RemoveState(AspirateState stateArg) => Resolve().RemoveState(stateArg);
    public bool SecretStateExists(AspirateState stateArg) => Resolve().SecretStateExists(stateArg);
    public string? GetSecret(string resourceName, string key) => Resolve().GetSecret(resourceName, key);
    public void SetPassword(string password) => Resolve().SetPassword(password);
    public bool CheckPassword(string password) => Resolve().CheckPassword(password);
    public void RotatePassword(string newPassword) => Resolve().RotatePassword(newPassword);
    public void ClearPassword() => Resolve().ClearPassword();
}

