using Spectre.Console;
using Aspirate.Shared.Models.Aspirate;
using Aspirate.Shared.Models.AspireManifests.Components.Common;
using Aspirate.Shared.Models.AspireManifests.Components.Common.Container;
using Aspirate.Shared.Models.AspireManifests.Components.V0;

namespace Aspirate.Commands.Actions.Manifests;

/// <summary>
/// Prompts the user to configure security context values for each container resource.
/// </summary>
public class ConfigureSecurityContextAction(IServiceProvider serviceProvider) : BaseActionWithNonInteractiveValidation(serviceProvider)
{
    public override Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Configuring Security Context[/]");

        if (PreviousStateWasRestored())
        {
            return Task.FromResult(true);
        }

        CurrentState.SecurityContexts ??= new();

        var candidates = CurrentState.AllSelectedSupportedComponents
            .Where(r => r.Value is ContainerResourceBase or ProjectResource or DockerfileResource)
            .Select(r => r.Key)
            .ToList();

        if (candidates.Count == 0)
        {
            Logger.MarkupLine("[yellow](!)[/] No container resources detected.");
            return Task.FromResult(true);
        }

        if (CurrentState.NonInteractive)
        {
            return Task.FromResult(true);
        }

        foreach (var name in candidates)
        {
            var runAsUserStr = Logger.Prompt(new TextPrompt<string>($"RunAsUser for [blue]{name}[/] (leave blank to skip): ").AllowEmpty());
            var runAsGroupStr = Logger.Prompt(new TextPrompt<string>($"RunAsGroup for [blue]{name}[/] (leave blank to skip): ").AllowEmpty());
            var fsGroupStr = Logger.Prompt(new TextPrompt<string>($"FsGroup for [blue]{name}[/] (leave blank to skip): ").AllowEmpty());
            var runAsNonRoot = Logger.Confirm($"Run as non root for [blue]{name}[/]?", false);

            var context = new PodSecurityContext();
            if (long.TryParse(runAsUserStr, out var runAsUser))
            {
                context.RunAsUser = runAsUser;
            }
            if (long.TryParse(runAsGroupStr, out var runAsGroup))
            {
                context.RunAsGroup = runAsGroup;
            }
            if (long.TryParse(fsGroupStr, out var fsGroup))
            {
                context.FsGroup = fsGroup;
            }
            context.RunAsNonRoot = runAsNonRoot;

            if (context.RunAsUser != null || context.RunAsGroup != null || context.FsGroup != null || context.RunAsNonRoot == true)
            {
                CurrentState.SecurityContexts[name] = context;
            }
        }

        return Task.FromResult(true);
    }

    public override void ValidateNonInteractiveState()
    {
        // No validation required
    }
}
