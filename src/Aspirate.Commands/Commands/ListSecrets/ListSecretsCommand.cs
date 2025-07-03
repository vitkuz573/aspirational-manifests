namespace Aspirate.Commands.Commands.ListSecrets;

public sealed class ListSecretsCommand : BaseCommand<ListSecretsOptions, ListSecretsCommandHandler>
{
    protected override bool CommandUnlocksSecrets => true;
    protected override bool CommandAlwaysRequiresState => true;

    public ListSecretsCommand() : base("list-secrets", "Lists secret keys per resource")
    {
        AddOption(ResourceOption.Instance);
        AddOption(ProviderOption.Instance);
    }
}
