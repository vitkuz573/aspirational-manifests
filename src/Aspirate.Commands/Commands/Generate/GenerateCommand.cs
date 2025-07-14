namespace Aspirate.Commands.Commands.Generate;

public sealed class GenerateCommand : BaseCommand<GenerateOptions, GenerateCommandHandler>
{
    protected override bool CommandUnlocksSecrets => true;

    public GenerateCommand() : base("generate", "Builds, pushes containers, generates aspire manifest, helm chart and kustomize manifests.")
    {
        Options.Add(ProjectPathOption.Instance);
        Options.Add(AspireManifestOption.Instance);
        Options.Add(OutputPathOption.Instance);
        Options.Add(SkipBuildOption.Instance);
        Options.Add(SkipFinalKustomizeGenerationOption.Instance);
        Options.Add(ContainerBuilderOption.Instance);
        Options.Add(ContainerBuildContextOption.Instance);
        Options.Add(ContainerImageTagOption.Instance);
        Options.Add(ContainerBuildArgsOption.Instance);
        Options.Add(PreferDockerfileOption.Instance);
        Options.Add(ContainerRegistryOption.Instance);
        Options.Add(ContainerRepositoryPrefixOption.Instance);
        Options.Add(ImagePullPolicyOption.Instance);
        Options.Add(NamespaceOption.Instance);
        Options.Add(OutputFormatOption.Instance);
        Options.Add(RuntimeIdentifierOption.Instance);
        Options.Add(SecretPasswordOption.Instance);
        Options.Add(PrivateRegistryOption.Instance);
        Options.Add(PrivateRegistryUrlOption.Instance);
        Options.Add(PrivateRegistryUsernameOption.Instance);
        Options.Add(PrivateRegistryPasswordOption.Instance);
        Options.Add(PrivateRegistryEmailOption.Instance);
        Options.Add(IncludeDashboardOption.Instance);
        Options.Add(ComposeBuildsOption.Instance);
        Options.Add(ReplaceSecretsOption.Instance);
        Options.Add(ParameterResourceValueOption.Instance);
        Options.Add(ComponentsOption.Instance);
        Options.Add(OverlayPathOption.Instance);
    }
}
