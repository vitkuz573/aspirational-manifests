namespace Aspirate.Commands.Options;

public sealed class RollingRestartOption : BaseOption<bool?>
{
    private static readonly string[] _aliases =
    [
        "-r",
        "--rolling-restart"
    ];

    private RollingRestartOption() : base(nameof(IApplyOptions.RollingRestart), _aliases, "ASPIRATE_ROLLING_RESTART", null)
    {
        Description = "Indicates if a rolling restart should occur at the end of deploy. Defaults to 'false'.";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static RollingRestartOption Instance { get; } = new();

    public override bool IsSecret => false;
}
