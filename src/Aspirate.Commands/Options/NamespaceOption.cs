namespace Aspirate.Commands.Options;

public sealed class NamespaceOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--namespace"
    ];

    private NamespaceOption() : base(nameof(IGenerateOptions.Namespace), _aliases, "ASPIRATE_NAMESPACE", null)
    {
        Description = "The Namespace to use for deployments";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static NamespaceOption Instance { get; } = new();

    public override bool IsSecret => false;
}
