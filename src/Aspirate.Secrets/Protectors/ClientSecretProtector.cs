namespace Aspirate.Secrets.Protectors;

public class ClientSecretProtector(ISecretProvider secretProvider, IAnsiConsole console) : BaseProtector(secretProvider, console)
{
    public override bool HasSecrets(KeyValuePair<string, Resource> component)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return false;
        }

        return componentWithEnv.Env?.Any(x => x.Key.Equals(ProtectorType.ClientSecret.Value, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public override void ProtectSecrets(KeyValuePair<string, Resource> component, bool nonInteractive)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return;
        }

        var clientSecretInput = componentWithEnv.Env.FirstOrDefault(x => x.Key.Equals(ProtectorType.ClientSecret.Value, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(clientSecretInput.Key) && clientSecretInput.Key.Equals(ProtectorType.ClientSecret.Value, StringComparison.OrdinalIgnoreCase))
        {
            UpsertSecret(component, clientSecretInput, nonInteractive);
        }
    }
}
