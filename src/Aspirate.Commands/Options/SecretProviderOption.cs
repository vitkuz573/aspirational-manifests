namespace Aspirate.Commands.Options;

public sealed class SecretProviderOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["--secret-provider"];

    private SecretProviderOption() : base(
        _aliases,
        "ASPIRATE_SECRET_PROVIDER",
        AspirateSecretLiterals.FileSecretsManager)
    {
        Name = nameof(ICommandOptions.SecretProvider);
        Description = "The secret backend provider to use. Defaults to file.";
        Arity = ArgumentArity.ExactlyOne;
        IsRequired = false;
    }

    public static SecretProviderOption Instance { get; } = new();

    public override bool IsSecret => false;
}

