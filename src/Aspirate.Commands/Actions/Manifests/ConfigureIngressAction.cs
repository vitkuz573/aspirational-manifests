using Aspirate.Shared.Literals;
using Aspirate.Shared.Extensions;
using Aspirate.Shared.Models.AspireManifests.Components.Common;

namespace Aspirate.Commands.Actions.Manifests;

public class ConfigureIngressAction(
    IKubernetesIngressService ingressService,
    IServiceProvider serviceProvider) : BaseActionWithNonInteractiveValidation(serviceProvider)
{
    public override async Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Configuring Ingress[/]");

        if (PreviousStateWasRestored())
        {
            return true;
        }

        var candidates = CurrentState.GetResourcesWithExternalBindings()
            .Select(r => r.Key)
            .ToList();


        if (candidates.Count == 0)
        {
            Logger.MarkupLine("[yellow](!)[/] No services with external bindings detected.");
            return true;
        }


        CurrentState.IngressDefinitions ??= new();

        if (!CurrentState.NonInteractive)
        {
            if (string.IsNullOrEmpty(CurrentState.IngressController))
            {
                var controller = Logger.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select ingress controller")
                        .PageSize(10)
                        .AddChoices(IngressController.List.Select(c => c.Value).ToArray()));

                CurrentState.IngressController = controller;
            }

            var selected = Logger.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select [green]services[/] to expose via ingress")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more services)[/]")
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle a service, " +
                        "[green]<enter>[/] to accept)[/]")
                    .AddChoiceGroup("All Services", candidates));

            foreach (var service in selected)
            {
                var hosts = new List<string>();

                while (true)
                {
                    var host = Logger.Prompt(
                        new TextPrompt<string>($"[bold]Enter host for service [blue]{service}[/] (leave blank to stop): [/]")
                            .PromptStyle("yellow")
                            .AllowEmpty());

                    if (string.IsNullOrWhiteSpace(host))
                    {
                        if (hosts.Count == 0)
                        {
                            Logger.MarkupLine("[red]A host is required.[/]");
                            continue;
                        }

                        break;
                    }

                    hosts.Add(host);
                }

                var tls = Logger.Prompt(
                    new TextPrompt<string>($"[bold]Enter TLS secret for service [blue]{service}[/] (leave blank if none): [/]")
                        .PromptStyle("yellow")
                        .AllowEmpty());

                var bindings = ((IResourceWithBinding)CurrentState.LoadedAspireManifestResources[service])
                    .Bindings!
                    .Where(b => b.Value.External)
                    .ToList();

                var selectedBinding = bindings.Count == 1
                    ? bindings[0]
                    : Logger.Prompt(
                        new SelectionPrompt<KeyValuePair<string, Binding>>()
                            .Title($"Select external binding for service [blue]{service}[/]")
                            .UseConverter(b => $"{b.Key} ({b.Value.Scheme}:{b.Value.TargetPort})")
                            .AddChoices(bindings));

                var port = selectedBinding.Value.Port ?? selectedBinding.Value.TargetPort;

                var annotations = new Dictionary<string, string>();
                while (true)
                {
                    var key = Logger.Prompt(new TextPrompt<string>($"Enter annotation key for [blue]{service}[/] (leave blank to stop): ")
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

                CurrentState.IngressDefinitions[service] = new IngressDefinition
                {
                    Hosts = hosts,
                    Path = "/",
                    TlsSecret = string.IsNullOrWhiteSpace(tls) ? null : tls,
                    PortNumber = port,
                    Annotations = annotations.Count > 0 ? annotations : null
                };
            }

            if (Logger.Confirm($"Deploy {CurrentState.IngressController} ingress controller if not present?", false) &&
                !string.IsNullOrEmpty(CurrentState.KubeContext))
            {
                await ingressService.EnsureIngressController(CurrentState.KubeContext, CurrentState.IngressController);
            }
        }

        return true;
    }

    public override void ValidateNonInteractiveState()
    {
        if (CurrentState.NonInteractive && (CurrentState.IngressDefinitions == null || CurrentState.IngressDefinitions.Count == 0))
        {
            Logger.ValidationFailed("Ingress definitions are required when running non-interactively.");
        }

        if (CurrentState.NonInteractive && string.IsNullOrEmpty(CurrentState.IngressController))
        {
            Logger.ValidationFailed("Ingress controller is required when running non-interactively.");
        }
    }
}
