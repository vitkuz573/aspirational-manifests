namespace Aspirate.Commands.Options;

public sealed class ContainerImageTagOption : BaseOption<List<string>?>
{
    private static readonly string[] _aliases =
    [
        "-ct",
        "--container-image-tag"
    ];

    private ContainerImageTagOption() : base(nameof(IContainerOptions.ContainerImageTags), _aliases, "ASPIRATE_CONTAINER_IMAGE_TAG", null)
    {
        Description = "The Container Image Tags to use for all containers. Can include multiple times.";
        Arity = ArgumentArity.ZeroOrMore;
        Required = false;
    }

    public static ContainerImageTagOption Instance { get; } = new();

    public override bool IsSecret => false;
}
