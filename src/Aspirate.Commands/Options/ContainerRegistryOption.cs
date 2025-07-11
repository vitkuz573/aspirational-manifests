namespace Aspirate.Commands.Options;

public sealed class ContainerRegistryOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-cr",
        "--container-registry"
    ];

    private ContainerRegistryOption() : base(_aliases, "ASPIRATE_CONTAINER_REGISTRY", null)
    {
        Name = nameof(IContainerOptions.ContainerRegistry);
        Description = "The Container Registry to use as the fall-back value for all containers";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ContainerRegistryOption Instance { get; } = new();

    public override bool IsSecret => false;
}
