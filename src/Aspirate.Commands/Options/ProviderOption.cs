namespace Aspirate.Commands.Options;

public sealed class ProviderOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["-p", "--provider"];

    private ProviderOption() : base(_aliases, "ASPIRATE_SECRET_PROVIDER_FILTER", null)
    {
        Name = nameof(IListSecretsOptions.Provider);
        Description = "Filter secrets by resource provider.";
        Arity = ArgumentArity.ExactlyOne;
        IsRequired = false;
    }

    public static ProviderOption Instance { get; } = new();

    public override bool IsSecret => false;
}
