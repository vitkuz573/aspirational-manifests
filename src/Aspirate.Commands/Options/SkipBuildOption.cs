namespace Aspirate.Commands.Options;

public sealed class SkipBuildOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--skip-build"];

    private SkipBuildOption() : base(nameof(IGenerateOptions.SkipBuild), _aliases, "ASPIRATE_SKIP_BUILD", null)
    {
        Description = "Skips build and Push of containers";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static SkipBuildOption Instance { get; } = new();

    public override bool IsSecret => false;
}
