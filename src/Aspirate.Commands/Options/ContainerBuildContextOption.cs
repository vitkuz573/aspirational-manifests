namespace Aspirate.Commands.Options;

public sealed class ContainerBuildContextOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-cbc",
        "--container-build-context"
    ];

    private ContainerBuildContextOption() : base(nameof(IContainerOptions.ContainerBuildContext), _aliases, "ASPIRATE_CONTAINER_BUILD_CONTEXT", null)
    {
        Description = "The Container Build Context to use when Dockerfile is used to build projects";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ContainerBuildContextOption Instance { get; } = new();

    public override bool IsSecret => false;
}
