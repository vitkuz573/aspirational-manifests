namespace Aspirate.Commands.Actions.Secrets;

public class SaveSecretsAction(
    IAnsiConsole console,
    ISecretService secretService,
    IServiceProvider serviceProvider) : BaseAction(serviceProvider)
{
    public override async Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Populating Secrets File[/]");

        await secretService.SaveSecretsAsync(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = CurrentState.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = CurrentState.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
        });

        return true;
    }
}
