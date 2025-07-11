namespace Aspirate.Commands.Options;

public sealed class InputPathOption : BaseOption<string>
{
    private static readonly string[] _aliases =
    [
        "-i",
        "--input-path"
    ];

    private InputPathOption() : base(nameof(IKubernetesOptions.InputPath), _aliases, "ASPIRATE_INPUT_PATH", AspirateLiterals.DefaultArtifactsPath)
    {
        Description = "The path for the kustomize manifests directory";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static InputPathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
