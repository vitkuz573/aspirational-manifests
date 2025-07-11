namespace Aspirate.Commands.Options;

public sealed class PrivateRegistryEmailOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--private-registry-email"
    ];

    private PrivateRegistryEmailOption() : base(nameof(IPrivateRegistryCredentialsOptions.PrivateRegistryEmail), _aliases, "ASPIRATE_PRIVATE_REGISTRY_EMAIL", "aspir8@aka.ms")
    {
        Description = "The Private Registry email. It is required and defaults to 'aspirate@aspirate.com'.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static PrivateRegistryEmailOption Instance { get; } = new();

    public override bool IsSecret => false;
}
