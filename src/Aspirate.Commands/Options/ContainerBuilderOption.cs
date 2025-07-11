namespace Aspirate.Commands.Options;

public sealed class ContainerBuilderOption : BaseOption<string>
{
    private static readonly string[] _aliases = ["--container-builder"];

    private ContainerBuilderOption() : base(nameof(IContainerOptions.ContainerBuilder), _aliases, "ASPIRATE_CONTAINER_BUILDER", "docker")
    {
        Description = "The Container Builder: can be 'docker', 'podman' or 'nerdctl'. The default is 'docker'";
        Arity = ArgumentArity.ExactlyOne;
        Required = false;

        Validators.Add(ValidateFormat);
    }

    public static ContainerBuilderOption Instance { get; } = new();

    public override bool IsSecret => false;

    private static void ValidateFormat(OptionResult optionResult)
    {
        var value = optionResult.GetValueOrDefault<string>();

        if (value is null)
        {
            throw new ArgumentException("--container-builder cannot be null.");
        }

        if (!ContainerBuilder.TryFromValue(value.ToLower(), out _))
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.Append("--container-builder must be one of: '");
            errorBuilder.AppendJoin("', '", ContainerBuilder.List.Select(x => x.Value));
            errorBuilder.Append("' and not quoted.");

            throw new ArgumentException(errorBuilder.ToString());
        }
    }
}
