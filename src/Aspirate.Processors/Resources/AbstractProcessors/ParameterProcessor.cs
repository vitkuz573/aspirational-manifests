namespace Aspirate.Processors.Resources.AbstractProcessors;

public class ParameterProcessor(IFileSystem fileSystem, IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    /// <inheritdoc />
    public override string ResourceType => AspireComponentLiterals.Parameter;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader)
    {
        var parameter = JsonSerializer.Deserialize<ParameterResource>(ref reader);

        ValidateParameterResource(parameter);

        return parameter;
    }

    private static void ValidateParameterResource(ParameterResource? parameter)
    {
        if (parameter == null)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} not found.");
        }

        if (parameter.Value is null)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameter.Name} missing required property 'value'.");
        }

        if (parameter.Inputs is null)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameter.Name} missing required property 'inputs'.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing for Parameter Resources, they are there for configuration.
        Task.FromResult(true);
}
