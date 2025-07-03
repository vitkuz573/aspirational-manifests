namespace Aspirate.Commands.Commands.ClearSecrets;

public sealed class ClearSecretsOptions : BaseCommandOptions, IClearSecretsOptions
{
    public bool? Force { get; set; }
}
