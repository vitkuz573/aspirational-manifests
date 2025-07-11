namespace Aspirate.Commands.Options;

public sealed class ComponentsOption : BaseOption<List<string>?>
{
    private static readonly string[] _aliases =
    [
        "-c",
        "--components"
    ];

    private ComponentsOption() : base(nameof(IComponentsOptions.CliSpecifiedComponents), _aliases, "ASPIRATE_COMPONENTS", null)
    {
        Description = "Specify which components build or generate, non interactively";
        Arity = ArgumentArity.ZeroOrMore;
        Required = false;
    }

    public static ComponentsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
