namespace Aspirate.Commands.Options;

public sealed class OutputPathOption : BaseOption<string>
{
    private static readonly string[] _aliases =
    [
        "-o",
        "--output-path"
    ];

    private OutputPathOption() : base(nameof(IGenerateOptions.OutputPath), _aliases, "ASPIRATE_OUTPUT_PATH", AspirateLiterals.DefaultArtifactsPath)
    {
        Description = "The output path for generated manifests";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static OutputPathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
