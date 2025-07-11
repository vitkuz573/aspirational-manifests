namespace Aspirate.Commands.Options;

public sealed class RuntimeIdentifierOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["--runtime-identifier"];

    private RuntimeIdentifierOption() : base(nameof(IBuildOptions.RuntimeIdentifier), _aliases, "ASPIRATE_RUNTIME_IDENTIFIER", null)
    {
        Description = "The Custom Runtime identifier to use for .net project builds.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static RuntimeIdentifierOption Instance { get; } = new();

    public override bool IsSecret => false;
}
