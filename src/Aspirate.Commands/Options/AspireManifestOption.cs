namespace Aspirate.Commands.Options;

public sealed class AspireManifestOption : BaseOption<string?>
{
    private static readonly string[] _aliases =
    [
        "-m",
        "--aspire-manifest"
    ];

    private AspireManifestOption() : base(nameof(IAspireOptions.AspireManifest), _aliases, "ASPIRATE_ASPIRE_MANIFEST_PATH", null)
    {
        Description = "The aspire manifest file to use";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;
    }

    public static AspireManifestOption Instance { get; } = new();

    public override bool IsSecret => false;
}
