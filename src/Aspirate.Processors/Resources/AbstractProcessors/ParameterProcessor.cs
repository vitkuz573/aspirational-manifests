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

        foreach (var input in parameter.Inputs)
        {
            ValidateInput(parameter.Name, input.Key, input.Value);
        }
    }

    private static void ValidateInput(string parameterName, string inputName, ParameterInput? input)
    {
        if (input == null)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' not found.");
        }

        if (string.IsNullOrWhiteSpace(input.Type))
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' missing required property 'type'.");
        }

        ValidateDefault(parameterName, inputName, input.Default);
    }

    private static void ValidateDefault(string parameterName, string inputName, ParameterDefault? defaultValue)
    {
        if (defaultValue == null)
        {
            return;
        }

        var hasGenerate = defaultValue.Generate is not null;
        var hasValue = defaultValue.Value is not null;

        if (hasGenerate == hasValue)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' default must specify either 'generate' or 'value'.");
        }

        if (hasGenerate && defaultValue.Generate!.MinLength <= 0)
        {
            throw new InvalidOperationException(
                $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minLength must be greater than 0.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing for Parameter Resources, they are there for configuration.
        Task.FromResult(true);
}
