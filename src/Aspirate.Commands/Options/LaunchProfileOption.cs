namespace Aspirate.Commands.Options;

public sealed class LaunchProfileOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-lp",
        "--launch-profile"
    ];

    private LaunchProfileOption() : base(nameof(ICommandOptions.LaunchProfile), _aliases, "ASPIRATE_LAUNCH_PROFILE", null)
    {
        Description = "The launch profile to use when building the aspire manifest from the AppHost.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static LaunchProfileOption Instance { get; } = new();

    public override bool IsSecret => false;
}
