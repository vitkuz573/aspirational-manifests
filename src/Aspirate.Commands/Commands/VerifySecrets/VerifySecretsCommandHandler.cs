namespace Aspirate.Commands.Commands.VerifySecrets;

public sealed class VerifySecretsCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<VerifySecretsOptions>(serviceProvider)
{
    public override async Task<int> HandleAsync(VerifySecretsOptions options)
    {
        var secretService = Services.GetRequiredService<ISecretService>();

        await secretService.VerifySecretsAsync(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = options.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = options.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
            Pbkdf2Iterations = options.Pbkdf2Iterations,
            StatePath = options.StatePath ?? Directory.GetCurrentDirectory(),
        });

        return 0;
    }
}
