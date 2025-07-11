namespace Aspirate.Commands.Options;

public sealed class ContainerBuildArgsOption : BaseOption<List<string>?>
{
    private static readonly string[] _aliases =
    [
        "-cba",
        "--container-build-arg"
    ];

    private ContainerBuildArgsOption() : base(nameof(IContainerOptions.ContainerBuildArgs), _aliases, "ASPIRATE_CONTAINER_BUILD_ARGS", null)
    {
        Description = "The Container Build Arguments to use for all containers. In \"key\"=\"value\" format. Can include multiple times.";
        Arity = ArgumentArity.ZeroOrMore;
        Required = false;
    }

    public static ContainerBuildArgsOption Instance { get; } = new();

    public override bool IsSecret => false;
}
