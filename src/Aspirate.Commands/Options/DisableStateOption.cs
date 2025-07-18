namespace Aspirate.Commands.Options;

public sealed class DisableStateOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--disable-state", "--no-state"];

    private DisableStateOption() : base(nameof(ICommandOptions.DisableState), _aliases, "ASPIRATE_DISABLE_STATE", null)
    {
        Description = "Disables State Support";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static DisableStateOption Instance { get; } = new();

    public override bool IsSecret => false;
}
