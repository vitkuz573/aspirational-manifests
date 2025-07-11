namespace Aspirate.Commands.Options;

public sealed class AllowClearNamespaceOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--clear-namespace"];

    private AllowClearNamespaceOption() : base(nameof(IRunOptions.AllowClearNamespace), _aliases, "ASPIRATE_ALLOW_CLEAR_NAMESPACE", null)
    {
        Description = "Is Aspirate allowed to clear the namespace if it exists before deploying during the run command?";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static AllowClearNamespaceOption Instance { get; } = new();

    public override bool IsSecret => false;
}
