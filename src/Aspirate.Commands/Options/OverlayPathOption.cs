namespace Aspirate.Commands.Options;

public sealed class OverlayPathOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-op",
        "--overlay-path"
    ];

    private OverlayPathOption() : base(nameof(IGenerateOptions.OverlayPath), _aliases, "ASPIRATE_OVERLAY_PATH", null)
    {
        Description = "The path to a kustomize overlay directory";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static OverlayPathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
