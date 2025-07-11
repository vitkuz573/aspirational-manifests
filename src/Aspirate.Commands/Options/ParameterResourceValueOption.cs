namespace Aspirate.Commands.Options;

public sealed class ParameterResourceValueOption : BaseOption<List<string>?>
{
    private static readonly string[] _aliases =
    [
        "-pa",
        "--parameter"
    ];

    private ParameterResourceValueOption() : base(nameof(IGenerateOptions.Parameters), _aliases, "ASPIRATE_PARAMETER_VALUE", null)
    {
        Description = "The parameter resource value.";
        Arity = ArgumentArity.ZeroOrMore;
        Required = false;
    }

    public static ParameterResourceValueOption Instance { get; } = new();

    public override bool IsSecret => false;
}
