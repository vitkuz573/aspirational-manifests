namespace Aspirate.Commands.Commands.RotatePassword;

public sealed class RotatePasswordCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<RotatePasswordOptions>(serviceProvider)
{
    public override async Task<int> HandleAsync(RotatePasswordOptions options)
    {
        var secretService = Services.GetRequiredService<ISecretService>();

        await secretService.RotatePasswordAsync(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = options.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = options.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
            Pbkdf2Iterations = options.Pbkdf2Iterations,
        });

        return 0;
    }
}
