namespace Aspirate.Commands.Commands.Init;

public sealed class InitCommand : BaseCommand<InitOptions, InitCommandHandler>
{
    protected override bool CommandUnlocksSecrets => false;
    protected override bool CommandSkipsStateAndSecrets => true;

    public InitCommand() : base("init", "Initializes aspirate settings within your AppHost directory.")
    {
        Options.Add(ProjectPathOption.Instance);
        Options.Add(ContainerBuilderOption.Instance);
        Options.Add(ContainerBuildArgsOption.Instance);
        Options.Add(ContainerBuildContextOption.Instance);
        Options.Add(ContainerRegistryOption.Instance);
        Options.Add(ContainerRepositoryPrefixOption.Instance);
        Options.Add(ContainerImageTagOption.Instance);
        Options.Add(TemplatePathOption.Instance);
    }
}
