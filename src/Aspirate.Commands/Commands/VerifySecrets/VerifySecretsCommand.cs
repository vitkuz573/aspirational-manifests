namespace Aspirate.Commands.Commands.VerifySecrets;

public sealed class VerifySecretsCommand : BaseCommand<VerifySecretsOptions, VerifySecretsCommandHandler>
{
    protected override bool CommandUnlocksSecrets => true;
    protected override bool CommandAlwaysRequiresState => true;

    public VerifySecretsCommand() : base("verify-secrets", "Verifies the password protecting secrets")
    {
    }
}
