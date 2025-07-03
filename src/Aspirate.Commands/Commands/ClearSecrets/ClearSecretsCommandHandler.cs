namespace Aspirate.Commands.Commands.ClearSecrets;

public sealed class ClearSecretsCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<ClearSecretsOptions>(serviceProvider)
{
    public override Task<int> HandleAsync(ClearSecretsOptions options)
    {
        var secretService = Services.GetRequiredService<ISecretService>();

        secretService.ClearSecrets(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = options.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = options.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
            StatePath = options.StatePath ?? Directory.GetCurrentDirectory(),
            Force = options.Force
        });

        return Task.FromResult(0);
    }
}
