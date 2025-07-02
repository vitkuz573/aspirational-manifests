namespace Aspirate.Commands.Commands.RotatePassword;

public sealed class RotatePasswordCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<RotatePasswordOptions>(serviceProvider)
{
    public override Task<int> HandleAsync(RotatePasswordOptions options)
    {
        var secretService = Services.GetRequiredService<ISecretService>();

        secretService.RotatePassword(new SecretManagementOptions
        {
            State = CurrentState,
            NonInteractive = options.NonInteractive,
            DisableSecrets = CurrentState.DisableSecrets,
            SecretPassword = options.SecretPassword,
            SecretProvider = CurrentState.SecretProvider,
        });

        return Task.FromResult(0);
    }
}
