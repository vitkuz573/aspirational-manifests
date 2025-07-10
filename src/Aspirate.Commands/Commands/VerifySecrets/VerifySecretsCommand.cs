namespace Aspirate.Commands.Commands.VerifySecrets;

public sealed class VerifySecretsCommand : BaseCommand<VerifySecretsOptions, VerifySecretsCommandHandler>
{
    // Avoid unlocking secrets in BaseCommand to prevent duplicate password prompts.
    protected override bool CommandUnlocksSecrets => false;
    protected override bool CommandAlwaysRequiresState => true;

    public VerifySecretsCommand() : base("verify-secrets", "Verifies the password protecting secrets")
    {
    }
}
