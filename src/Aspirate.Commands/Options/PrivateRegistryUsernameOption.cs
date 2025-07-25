namespace Aspirate.Commands.Options;

public sealed class PrivateRegistryUsernameOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--private-registry-username"
    ];

    private PrivateRegistryUsernameOption() : base(nameof(IPrivateRegistryCredentialsOptions.PrivateRegistryUsername), _aliases, "ASPIRATE_PRIVATE_REGISTRY_USERNAME", null)
    {
        Description = "The Private Registry username.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static PrivateRegistryUsernameOption Instance { get; } = new();

    public override bool IsSecret => false;
}
