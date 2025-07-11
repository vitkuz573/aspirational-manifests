namespace Aspirate.Commands.Options;

public sealed class ContainerRepositoryPrefixOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "--container-repository-prefix",
        "-crp"
    ];

    private ContainerRepositoryPrefixOption() : base(nameof(IContainerOptions.ContainerRepositoryPrefix), _aliases, "ASPIRATE_CONTAINER_REPOSITORY_PREFIX", null)
    {
        Description = "The Container repository prefix to use as the fall-back value for all containers";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ContainerRepositoryPrefixOption Instance { get; } = new();

    public override bool IsSecret => false;
}
