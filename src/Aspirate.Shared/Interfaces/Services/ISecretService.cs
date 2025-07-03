namespace Aspirate.Shared.Interfaces.Services;

public interface ISecretService
{
    void LoadSecrets(SecretManagementOptions options);
    Task LoadSecretsAsync(SecretManagementOptions options);
    void SaveSecrets(SecretManagementOptions options);
    Task SaveSecretsAsync(SecretManagementOptions options);
    void ReInitialiseSecrets(SecretManagementOptions options);
    Task ReInitialiseSecretsAsync(SecretManagementOptions options);
    void RotatePassword(SecretManagementOptions options);
    Task RotatePasswordAsync(SecretManagementOptions options);
    void ClearSecrets(SecretManagementOptions options);
    Task ClearSecretsAsync(SecretManagementOptions options);
    void VerifySecrets(SecretManagementOptions options);
    Task VerifySecretsAsync(SecretManagementOptions options);
}
