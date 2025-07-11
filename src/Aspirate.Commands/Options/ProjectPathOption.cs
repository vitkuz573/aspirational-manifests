namespace Aspirate.Commands.Options;

public sealed class ProjectPathOption : BaseOption<string>
{
    private static readonly string[] _aliases =
    [
        "-p",
        "--project-path"
    ];

    private ProjectPathOption() : base(nameof(IAspireOptions.ProjectPath), _aliases, "ASPIRATE_PROJECT_PATH", AspirateLiterals.DefaultAspireProjectPath)
    {
        Description = "The path to the aspire project";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ProjectPathOption Instance { get; } = new();

    public override bool IsSecret => false;
}
