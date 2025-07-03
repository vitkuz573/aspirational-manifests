namespace Aspirate.Commands.Commands.ListSecrets;

public sealed class ListSecretsOptions : BaseCommandOptions, IListSecretsOptions
{
    public string? Resource { get; set; }
    public string? Provider { get; set; }
}
