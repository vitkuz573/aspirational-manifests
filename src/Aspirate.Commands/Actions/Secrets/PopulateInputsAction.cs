using Aspirate.Shared.Literals;

namespace Aspirate.Commands.Actions.Secrets;

public sealed class PopulateInputsAction(
    IPasswordGenerator passwordGenerator,
    IServiceProvider serviceProvider,
    ISecretProvider secretProvider) : BaseAction(serviceProvider)
{
    public override Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Handling Inputs[/]");

        var parameterResources = CurrentState.LoadedAspireManifestResources.Where(x => x.Value is ParameterResource).ToArray();

        ValidateInputTypes(parameterResources);

        if (parameterResources.Length == 0)
        {
            return Task.FromResult(true);
        }

        var parametersOverride = CurrentState.Parameters?.Select(s =>
            {
                var parts = s.Split('=', 2);
                return new KeyValuePair<string, string>(parts[0], parts[1]);
            })
            .ToDictionary() ?? new Dictionary<string, string>();

        var nonOverriddenParameterResources = parameterResources.Where(x => !parametersOverride.ContainsKey(x.Key)).ToArray();

        ApplyGeneratedValues(nonOverriddenParameterResources);

        ApplyOverriddenValues(parameterResources, parametersOverride);

        ApplyManualValues(nonOverriddenParameterResources);

        Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Input values have all been assigned.");

        return Task.FromResult(true);
    }

    private void ApplyOverriddenValues(KeyValuePair<string, Resource>[] parameterResources, Dictionary<string, string> overriddenValues)
    {
        foreach (var parameterResource in parameterResources)
        {
            if (overriddenValues.TryGetValue(parameterResource.Key, out var value))
            {
                var componentWithInput = parameterResource.Value as ParameterResource;
                componentWithInput.Value = value;
                Logger.MarkupLine($"[green]Overridden[/] value for [blue]{componentWithInput.Name}[/]");
            }
        }
    }

    private void ApplyManualValues(KeyValuePair<string, Resource>[] parameterResources)
    {
        foreach (var component in parameterResources)
        {
            var componentWithInput = component.Value as ParameterResource;

            var manualInputs = componentWithInput.Inputs?.Where(x => x.Value.Default is null);

            AssignManualValues(ref manualInputs, componentWithInput);
        }
    }

    private void ApplyGeneratedValues(KeyValuePair<string, Resource>[] parameterResources)
    {
        foreach (var component in parameterResources)
        {
            var componentWithInput = component.Value as ParameterResource;

            var generatedInputs = componentWithInput.Inputs?.Where(x => x.Value.Default is not null);

            AssignGeneratedValues(ref generatedInputs, componentWithInput);
        }
    }

    private void AssignManualValues(ref IEnumerable<KeyValuePair<string, ParameterInput>>? manualInputs, ParameterResource parameterResource)
    {
        if (manualInputs is null)
        {
            return;
        }

        foreach (var input in manualInputs)
        {
            HandleSetInput(input, parameterResource);
        }
    }

    private void HandleSetInput(KeyValuePair<string, ParameterInput> input, ParameterResource parameterResource)
    {
        if (AssignExistingSecret(input, parameterResource))
        {
            return;
        }

        if (CurrentState.NonInteractive)
        {
            Logger.ValidationFailed("Cannot obtain non-generated values for inputs in non-interactive mode. Inputs are required according to the manifest.");
            ActionCausesExitException.ExitNow();
        }

        var firstPrompt = new TextPrompt<string>($"Enter a value for resource [blue]{parameterResource.Name}'s[/] Input Value [blue]'{input.Key}'[/]: ").PromptStyle("yellow");
        var secondPrompt = new TextPrompt<string>("Please repeat the value: ").PromptStyle("yellow");

        var firstInput = Logger.Prompt(firstPrompt);
        var secondInput = Logger.Prompt(secondPrompt);

        if (firstInput.Equals(secondInput, StringComparison.Ordinal))
        {
            parameterResource.Value = firstInput;
            AddParameterInputToSecretStore(input, parameterResource, firstInput);
            Logger.MarkupLine($"Successfully [green]assigned[/] a value for [blue]{parameterResource.Name}'s[/] Input Value [blue]'{input.Key}'[/]");
            return;
        }

        Logger.MarkupLine("[red]The values do not match. Please try again.[/]");
        HandleSetInput(input, parameterResource);
    }

    private bool AssignExistingSecret(KeyValuePair<string, ParameterInput> input, ParameterResource parameterResource)
    {
        if (!input.Value.Secret || CurrentState.ReplaceSecrets == true || CurrentState.DisableSecrets == true || !secretProvider.SecretStateExists(CurrentState) || !secretProvider.ResourceExists(parameterResource.Name) ||
            !secretProvider.SecretExists(parameterResource.Name, input.Key))
        {
            return false;
        }

        parameterResource.Value = secretProvider.GetSecret(parameterResource.Name, input.Key);
        Logger.MarkupLine(
            $"[green]Secret[/] for [blue]{parameterResource.Name}'s[/] Input Value [blue]'{input.Key}'[/] loaded from secret state.");

        return true;
    }

    private void AddParameterInputToSecretStore(KeyValuePair<string, ParameterInput> input, ParameterResource parameterResource, string valueToStore)
    {
        if (CurrentState.DisableSecrets == true || !input.Value.Secret)
        {
            return;
        }

        if (!secretProvider.ResourceExists(parameterResource.Name))
        {
            secretProvider.AddResource(parameterResource.Name);
        }

        secretProvider.AddSecret(parameterResource.Name, input.Key, valueToStore);
    }

    private void AssignGeneratedValues(ref IEnumerable<KeyValuePair<string, ParameterInput>>? generatedInputs, ParameterResource parameterResource)
    {
        if (generatedInputs is null)
        {
            return;
        }

        foreach (var input in generatedInputs)
        {
            if (AssignExistingSecret(input, parameterResource))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(input.Value.Default?.Value))
            {
                parameterResource.Value = input.Value.Default!.Value;
            }
            else
            {
                var options = input.Value.Default?.Generate ?? new Generate { MinLength = 22 };
                parameterResource.Value = passwordGenerator.Generate(options);
            }

            AddParameterInputToSecretStore(input, parameterResource, parameterResource.Value);

            Logger.MarkupLine($"Successfully [green]generated[/] a value for [blue]{parameterResource.Name}'s[/] Input Value [blue]'{input.Key}'[/]");
        }
    }

    private void ValidateInputTypes(KeyValuePair<string, Resource>[] parameterResources)
    {
        foreach (var parameterResource in parameterResources)
        {
            var resource = parameterResource.Value as ParameterResource;

            if (resource.Inputs is null)
            {
                continue;
            }

            foreach (var input in resource.Inputs)
            {
                if (!string.Equals(input.Value.Type, ParameterInputLiterals.String, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.ValidationFailed($"Invalid parameter input type '{input.Value.Type}' for '{resource.Name}.{input.Key}'. Only '{ParameterInputLiterals.String}' is supported.");
                }
            }
        }
    }
}
