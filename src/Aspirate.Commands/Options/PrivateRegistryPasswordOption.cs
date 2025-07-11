namespace Aspirate.Commands.Options;

public sealed class PrivateRegistryPasswordOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--private-registry-password"
    ];

    private PrivateRegistryPasswordOption() : base(nameof(IPrivateRegistryCredentialsOptions.PrivateRegistryPassword), _aliases, "ASPIRATE_PRIVATE_REGISTRY_PASSWORD", null)
    {
        Description = "The Private Registry password.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static PrivateRegistryPasswordOption Instance { get; } = new();

    public override bool IsSecret => true;
}
