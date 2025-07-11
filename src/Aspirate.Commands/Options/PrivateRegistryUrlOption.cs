namespace Aspirate.Commands.Options;

public sealed class PrivateRegistryUrlOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--private-registry-url"
    ];

    private PrivateRegistryUrlOption() : base(nameof(IPrivateRegistryCredentialsOptions.PrivateRegistryUrl), _aliases, "ASPIRATE_PRIVATE_REGISTRY_URL", null)
    {
        Description = "The Private Registry url.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static PrivateRegistryUrlOption Instance { get; } = new();

    public override bool IsSecret => false;
}
