namespace Aspirate.Commands.Commands.ClearSecrets;

public sealed class ClearSecretsCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<ClearSecretsOptions>(serviceProvider)
{
    public override async Task<int> HandleAsync(ClearSecretsOptions options)
    {
        var secretService = Services.GetRequiredService<ISecretService>();

        await secretService.ClearSecretsAsync(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = options.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = options.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
            Pbkdf2Iterations = options.Pbkdf2Iterations,
            StatePath = options.StatePath ?? Directory.GetCurrentDirectory(),
            Force = options.Force
        });

        return 0;
    }
}
