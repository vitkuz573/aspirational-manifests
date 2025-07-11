namespace Aspirate.Commands.Commands.Apply;

public sealed class ApplyCommand : BaseCommand<ApplyOptions, ApplyCommandHandler>
{
    protected override bool CommandUnlocksSecrets => true;

    public ApplyCommand() : base("apply", "Apply the generated kustomize manifest to the cluster.")
    {
        Options.Add(InputPathOption.Instance);
        Options.Add(KubernetesContextOption.Instance);
        Options.Add(OverlayPathOption.Instance);
        Options.Add(SecretPasswordOption.Instance);
        Options.Add(RollingRestartOption.Instance);
    }
}
