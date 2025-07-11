namespace Aspirate.Commands.Options;

public sealed class NonInteractiveOption : BaseOption<bool?>
{
    private static readonly string[] _aliases =
    [
        "--non-interactive"
    ];

    private NonInteractiveOption() : base(nameof(ICommandOptions.NonInteractive), _aliases, "ASPIRATE_NON_INTERACTIVE", null)
    {
        Description = "Disables interactive mode for the command";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static NonInteractiveOption Instance { get; } = new();

    public override bool IsSecret => false;
}
