namespace Aspirate.Commands.Options;

public sealed class OverlayPathOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-ol",
        "--overlay-path"
    ];

    private OverlayPathOption() : base(nameof(IKubernetesOptions.OverlayPath), _aliases, "ASPIRATE_OVERLAY_PATH", null)
    {
        Description = "Optional kustomize overlay directory";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static OverlayPathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
