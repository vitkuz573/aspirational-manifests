namespace Aspirate.Commands.Options;

public sealed class ResourceOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["-r", "--resource"];

    private ResourceOption() : base(nameof(IListSecretsOptions.Resource), _aliases, "ASPIRATE_SECRET_RESOURCE", null)
    {
        Description = "Filter secrets by resource name.";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static ResourceOption Instance { get; } = new();

    public override bool IsSecret => false;
}
