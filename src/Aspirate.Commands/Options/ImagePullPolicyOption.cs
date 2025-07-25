namespace Aspirate.Commands.Options;

public sealed class ImagePullPolicyOption : BaseOption<string?>
{
    private static readonly string[] _aliases = ["--image-pull-policy"];

    private ImagePullPolicyOption() : base(nameof(IGenerateOptions.ImagePullPolicy), _aliases, "ASPIRATE_IMAGE_PULL_POLICY", null)
    {
        Description = "The Image pull policy to use when generating manifests";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;

        Validators.Add(ValidateFormat);
    }

    public static ImagePullPolicyOption Instance { get; } = new();

    public override bool IsSecret => false;

    private static void ValidateFormat(OptionResult optionResult)
    {
        var value = optionResult.GetValueOrDefault<string?>();

        if (value is null)
        {
            return;
        }

        if (!ImagePullPolicy.TryFromValue(value, out _))
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.Append("--image-pull-policy must be one of: '");
            errorBuilder.AppendJoin("', '", ImagePullPolicy.List.Select(x => x.Value));
            errorBuilder.Append("' and not quoted. It is case sensitive.");

            throw new ArgumentException(errorBuilder.ToString());
        }
    }
}
