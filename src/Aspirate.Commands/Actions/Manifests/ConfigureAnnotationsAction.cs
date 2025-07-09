using Spectre.Console;
using Aspirate.Shared.Models.AspireManifests.Interfaces;

namespace Aspirate.Commands.Actions.Manifests;

public class ConfigureAnnotationsAction(IServiceProvider serviceProvider) : BaseActionWithNonInteractiveValidation(serviceProvider)
{
    public override Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Configuring Annotations[/]");

        if (PreviousStateWasRestored())
        {
            return Task.FromResult(true);
        }

        CurrentState.ResourceAnnotations ??= new();

        var candidates = CurrentState.AllSelectedSupportedComponents
            .Where(r => r.Value is IResourceWithAnnotations)
            .Select(r => r.Key)
            .ToList();

        if (candidates.Count == 0)
        {
            Logger.MarkupLine("[yellow](!)[/] No resources support annotations.");
            return Task.FromResult(true);
        }

        if (CurrentState.NonInteractive)
        {
            return Task.FromResult(true);
        }

        foreach (var resource in candidates)
        {
            var annotations = new Dictionary<string, string>();
            while (true)
            {
                var key = Logger.Prompt(new TextPrompt<string>($"Enter annotation key for [blue]{resource}[/] (leave blank to stop): ")
                    .PromptStyle("yellow")
                    .AllowEmpty());
                if (string.IsNullOrWhiteSpace(key))
                {
                    break;
                }

                var value = Logger.Prompt(new TextPrompt<string>($"Enter value for annotation [blue]{key}[/]: ")
                    .PromptStyle("yellow"));
                annotations[key] = value;
            }

            if (annotations.Count > 0)
            {
                CurrentState.ResourceAnnotations[resource] = annotations;
            }
        }

        return Task.FromResult(true);
    }

    public override void ValidateNonInteractiveState()
    {
        // Nothing to validate
    }
}
