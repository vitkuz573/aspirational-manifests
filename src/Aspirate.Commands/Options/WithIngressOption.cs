namespace Aspirate.Commands.Options;

public sealed class WithIngressOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--with-ingress"];

    private WithIngressOption() : base(_aliases, "ASPIRATE_WITH_INGRESS", null)
    {
        Name = nameof(IIngressOptions.WithIngress);
        Description = "Configure ingress resources for HTTP services";
        Arity = ArgumentArity.ZeroOrOne;
        IsRequired = false;
    }

    public static WithIngressOption Instance { get; } = new();

    public override bool IsSecret => false;
}
