namespace Aspirate.Commands.Options;

public sealed class SecretProviderOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["--secret-provider"];

    private SecretProviderOption() : base(nameof(ICommandOptions.SecretProvider), _aliases, "ASPIRATE_SECRET_PROVIDER", AspirateSecretLiterals.FileSecretsManager)
    {
        Description = "The secret backend provider to use. Defaults to file.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static SecretProviderOption Instance { get; } = new();

    public override bool IsSecret => false;
}
