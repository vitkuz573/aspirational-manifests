namespace Aspirate.Commands.Options;

public sealed class SecretPasswordOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--secret-password"
    ];

    private SecretPasswordOption() : base(nameof(ICommandOptions.SecretPassword), _aliases, "ASPIRATE_SECRET_PASSWORD", null)
    {
        Description = "The Secret Password to use";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static SecretPasswordOption Instance { get; } = new();

    public override bool IsSecret => true;
}
