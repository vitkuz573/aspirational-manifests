namespace Aspirate.Commands.Options;

public sealed class ComposeBuildsOption : BaseOption<List<string>?>
{
    private static readonly string[] _aliases = ["--compose-build"];

    private ComposeBuildsOption() : base(nameof(IBuildOptions.ComposeBuilds), _aliases, "ASPIRATE_COMPOSE_BUILDS", null)
    {
        Description = "Specify the resource names which will be built by the compose file.";
        Arity = ArgumentArity.ZeroOrMore;
        Required = false;
    }

    public static ComposeBuildsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
