namespace Aspirate.Commands.Options;

public sealed class Pbkdf2IterationsOption : BaseOption<int>
{
    private static readonly string[] _aliases = ["--pbkdf2-iterations"];

    private Pbkdf2IterationsOption() : base(_aliases, "ASPIRATE_PBKDF2_ITERATIONS", 1_000_000)
    {
        Name = nameof(ICommandOptions.Pbkdf2Iterations);
        Description = "The number of iterations for PBKDF2 when protecting secrets";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static Pbkdf2IterationsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
