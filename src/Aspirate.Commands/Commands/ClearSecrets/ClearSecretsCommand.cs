namespace Aspirate.Commands.Commands.ClearSecrets;

public sealed class ClearSecretsCommand : BaseCommand<ClearSecretsOptions, ClearSecretsCommandHandler>
{
    protected override bool CommandUnlocksSecrets => false;
    protected override bool CommandAlwaysRequiresState => true;

    public ClearSecretsCommand() : base("clear-secrets", "Removes stored secret state")
    {
        Options.Add(ForceOption.Instance);
    }
}
