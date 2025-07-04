namespace Aspirate.Commands.Options;

public sealed class StatePathOption : BaseOption<string>
{
    private static readonly string[] _aliases = ["--state-path"];

    private StatePathOption() : base(_aliases, "ASPIRATE_STATE_PATH", AspirateLiterals.DefaultAspireProjectPath)
    {
        Name = nameof(ICommandOptions.StatePath);
        Description = "The path where the state file will be stored";
        Arity = ArgumentArity.ExactlyOne;
        IsRequired = false;
    }

    public static StatePathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
