namespace Aspirate.Commands.Options;

public sealed class DisableSecretsOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--disable-secrets"];

    private DisableSecretsOption() : base(nameof(ICommandOptions.DisableSecrets), _aliases, "ASPIRATE_DISABLE_SECRETS", null)
    {
        Description = "Disables Secret Support";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static DisableSecretsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
