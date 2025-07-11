namespace Aspirate.Commands.Options;

public sealed class ReplaceSecretsOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--replace-secrets"];

    private ReplaceSecretsOption() : base(nameof(ISecretState.ReplaceSecrets), _aliases, "ASPIRATE_REPLACE_SECRETS", null)
    {
        Description = "Replace all secrets and inputs.";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static ReplaceSecretsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
