namespace Aspirate.Commands.Options;

public sealed class StatePathOption : BaseOption<string>
{
    private static readonly string[] _aliases = ["--state-path"];

    private StatePathOption() : base(nameof(ICommandOptions.StatePath), _aliases, "ASPIRATE_STATE_PATH", AspirateLiterals.DefaultAspireProjectPath)
    {
        Description = "The path where the state file will be stored";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static StatePathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
