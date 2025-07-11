namespace Aspirate.Commands.Options;

public sealed class KubernetesContextOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-k",
        "--kube-context"
    ];

    private KubernetesContextOption() : base(nameof(IKubernetesOptions.KubeContext), _aliases, "ASPIRATE_KUBERNETES_CONTEXT", null)
    {
        Description = "The name of the kubernetes context to use";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static KubernetesContextOption Instance { get; } = new();

    public override bool IsSecret => false;
}
