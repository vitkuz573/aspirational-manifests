namespace Aspirate.Commands.Options;

public sealed class TemplatePathOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-tp",
        "--template-path"
    ];

    private TemplatePathOption() : base(nameof(IInitOptions.TemplatePath), _aliases, "ASPIRATE_TEMPLATE_PATH", null)
    {
        Description = "The Custom Template path to use";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static TemplatePathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
