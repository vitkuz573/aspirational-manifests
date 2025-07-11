namespace Aspirate.Commands.Commands.Run;

public sealed class RunCommand : BaseCommand<RunOptions, RunCommandHandler>
{
    protected override bool CommandUnlocksSecrets => true;

    public RunCommand() : base("run", "Builds, pushes containers, and runs the current solution directly against a kubernetes cluster.")
    {
        Options.Add(ProjectPathOption.Instance);
        Options.Add(AspireManifestOption.Instance);
        Options.Add(SkipBuildOption.Instance);
        Options.Add(ContainerBuilderOption.Instance);
        Options.Add(ContainerBuildContextOption.Instance);
        Options.Add(ContainerImageTagOption.Instance);
        Options.Add(ContainerBuildArgsOption.Instance);
        Options.Add(PreferDockerfileOption.Instance);
        Options.Add(ContainerRegistryOption.Instance);
        Options.Add(ContainerRepositoryPrefixOption.Instance);
        Options.Add(ImagePullPolicyOption.Instance);
        Options.Add(NamespaceOption.Instance);
        Options.Add(OverlayPathOption.Instance);
        Options.Add(RuntimeIdentifierOption.Instance);
        Options.Add(SecretPasswordOption.Instance);
        Options.Add(PrivateRegistryOption.Instance);
        Options.Add(PrivateRegistryUrlOption.Instance);
        Options.Add(PrivateRegistryUsernameOption.Instance);
        Options.Add(PrivateRegistryPasswordOption.Instance);
        Options.Add(PrivateRegistryEmailOption.Instance);
        Options.Add(IncludeDashboardOption.Instance);
        Options.Add(AllowClearNamespaceOption.Instance);
    }
}
