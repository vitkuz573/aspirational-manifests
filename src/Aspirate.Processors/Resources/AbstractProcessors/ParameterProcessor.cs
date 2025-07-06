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

        if (hasGenerate)
        {
            var generate = defaultValue.Generate!;

            if (generate.MinLength <= 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minLength must be greater than 0.");
            }

            if (!generate.Lower && !generate.Upper && !generate.Numeric && !generate.Special)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate must allow at least one character type.");
            }

            if (generate.MinLower < 0 || generate.MinUpper < 0 || generate.MinNumeric < 0 || generate.MinSpecial < 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate minimum counts cannot be negative.");
            }

            if (!generate.Lower && generate.MinLower > 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minLower requires lower to be true.");
            }

            if (!generate.Upper && generate.MinUpper > 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minUpper requires upper to be true.");
            }

            if (!generate.Numeric && generate.MinNumeric > 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minNumeric requires numeric to be true.");
            }

            if (!generate.Special && generate.MinSpecial > 0)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' generate.minSpecial requires special to be true.");
            }

            var totalMin = generate.MinLower + generate.MinUpper + generate.MinNumeric + generate.MinSpecial;
            if (totalMin > generate.MinLength)
            {
                throw new InvalidOperationException(
                    $"{AspireComponentLiterals.Parameter} {parameterName} input '{inputName}' sum of minimum counts cannot exceed minLength.");
            }
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Do nothing for Parameter Resources, they are there for configuration.
        Task.FromResult(true);
}
