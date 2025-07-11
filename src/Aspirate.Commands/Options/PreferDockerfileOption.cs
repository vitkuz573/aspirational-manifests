namespace Aspirate.Commands.Options;

public sealed class PreferDockerfileOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--prefer-dockerfile"];

    private PreferDockerfileOption() : base(nameof(IBuildOptions.PreferDockerfile), _aliases, "ASPIRATE_PREFER_DOCKERFILE", null)
    {
        Description = "Instructs to use Dockerfile when available to build project images";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static PreferDockerfileOption Instance { get; } = new();

    public override bool IsSecret => false;
}
