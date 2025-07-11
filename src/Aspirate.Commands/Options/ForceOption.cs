namespace Aspirate.Commands.Options;

public sealed class ForceOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["-f", "--force"];

    private ForceOption() : base(nameof(IClearSecretsOptions.Force), _aliases, "ASPIRATE_FORCE", null)
    {
        Description = "Force the command to run without confirmation.";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static ForceOption Instance { get; } = new();

    public override bool IsSecret => false;
}
