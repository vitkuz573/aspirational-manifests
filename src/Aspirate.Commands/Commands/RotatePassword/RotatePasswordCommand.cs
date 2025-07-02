namespace Aspirate.Commands.Commands.RotatePassword;

public sealed class RotatePasswordCommand : BaseCommand<RotatePasswordOptions, RotatePasswordCommandHandler>
{
    protected override bool CommandUnlocksSecrets => false;
    protected override bool CommandAlwaysRequiresState => true;

    public RotatePasswordCommand() : base("rotate-password", "Rotates the password protecting secrets")
    {
    }
}
