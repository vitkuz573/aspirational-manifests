namespace Aspirate.Commands.Options;

public sealed class ForceOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["-f", "--force"];

    private ForceOption() : base(_aliases, "ASPIRATE_FORCE", null)
    {
        Name = nameof(IClearSecretsOptions.Force);
        Description = "Force the command to run without confirmation.";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static ForceOption Instance { get; } = new();

    public override bool IsSecret => false;
}
