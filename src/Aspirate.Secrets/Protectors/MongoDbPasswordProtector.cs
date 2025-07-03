namespace Aspirate.Secrets.Protectors;

public class MongoDbPasswordProtector(ISecretProvider secretProvider, IAnsiConsole console) : BaseProtector(secretProvider, console)
{
    public override bool HasSecrets(KeyValuePair<string, Resource> component)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return false;
        }

        return componentWithEnv.Env?.Any(x => x.Key.Equals(ProtectorType.MongoDbPassword.Value, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public override void ProtectSecrets(KeyValuePair<string, Resource> component, bool nonInteractive)
    {
        if (component.Value is not IResourceWithEnvironmentalVariables componentWithEnv)
        {
            return;
        }

        var input = componentWithEnv.Env.FirstOrDefault(x => x.Key.Equals(ProtectorType.MongoDbPassword.Value, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(input.Key) && input.Key.Equals(ProtectorType.MongoDbPassword.Value, StringComparison.OrdinalIgnoreCase))
        {
            UpsertSecret(component, input, nonInteractive);
        }
    }
}
