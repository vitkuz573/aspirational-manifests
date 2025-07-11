namespace Aspirate.Commands.Options;

public sealed class ProviderOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["-p", "--provider"];

    private ProviderOption() : base(nameof(IListSecretsOptions.Provider), _aliases, "ASPIRATE_SECRET_PROVIDER_FILTER", null)
    {
        Description = "Filter secrets by resource provider.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ProviderOption Instance { get; } = new();

    public override bool IsSecret => false;
}
