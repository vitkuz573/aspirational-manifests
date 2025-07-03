namespace Aspirate.Secrets.Protectors;

public class ApiKeyProtector(ISecretProvider secretProvider, IAnsiConsole console) : BaseProtector(secretProvider, console)
{
    public override bool HasSecrets(KeyValuePair<string, Resource> component)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return false;
        }

        return componentWithEnv.Env?.Any(x => x.Key.Equals(ProtectorType.ApiKey.Value, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public override void ProtectSecrets(KeyValuePair<string, Resource> component, bool nonInteractive)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return;
        }

        var apiKeyInput = componentWithEnv.Env.FirstOrDefault(x => x.Key.Equals(ProtectorType.ApiKey.Value, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(apiKeyInput.Key) && apiKeyInput.Key.Equals(ProtectorType.ApiKey.Value, StringComparison.OrdinalIgnoreCase))
        {
            UpsertSecret(component, apiKeyInput, nonInteractive);
        }
    }
}
