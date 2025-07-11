namespace Aspirate.Commands.Commands.Build;

public sealed class BuildCommand : BaseCommand<BuildOptions, BuildCommandHandler>
{
    protected override bool CommandUnlocksSecrets => false;

    public BuildCommand() : base("build", "Builds and pushes containers")
    {
        Options.Add(ProjectPathOption.Instance);
        Options.Add(AspireManifestOption.Instance);
        Options.Add(ContainerBuilderOption.Instance);
        Options.Add(ContainerBuildContextOption.Instance);
        Options.Add(ContainerImageTagOption.Instance);
        Options.Add(ContainerBuildArgsOption.Instance);
        Options.Add(PreferDockerfileOption.Instance);
        Options.Add(ContainerRegistryOption.Instance);
        Options.Add(ContainerRepositoryPrefixOption.Instance);
        Options.Add(RuntimeIdentifierOption.Instance);
        Options.Add(ComponentsOption.Instance);
    }
}
