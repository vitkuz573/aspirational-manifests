using Aspirate.Shared.Literals;

namespace Aspirate.Commands.Actions.Manifests;

public class ConfigureIngressAction(
    IKubernetesIngressService ingressService,
    IServiceProvider serviceProvider) : BaseActionWithNonInteractiveValidation(serviceProvider)
{
    public override async Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Configuring Ingress[/]");

        if (CurrentState.WithIngress == null)
        {
            if (CurrentState.NonInteractive)
            {
                Logger.ValidationFailed("The with ingress option is required in non-interactive mode.");
                ActionCausesExitException.ExitNow();
            }

            CurrentState.WithIngress = Logger.Confirm("[bold]Would you like to configure ingress for HTTP services?[/]", false);
        }

        if (CurrentState.WithIngress != true)
        {
            return true;
        }

        CurrentState.IngressDefinitions ??= new();

        var candidates = CurrentState.AllSelectedSupportedComponents
            .Where(r => r.Value is IResourceWithBinding res &&
                        res.Bindings != null &&
                        res.Bindings.ContainsKey(BindingLiterals.Http))
            .Select(r => r.Key)
            .ToList();

        if (candidates.Count == 0)
        {
            Logger.MarkupLine("[yellow](!)[/] No HTTP services detected.");
            return true;
        }

        if (!CurrentState.NonInteractive)
        {
            var selected = Logger.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select [green]services[/] to expose via ingress")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more services)[/]")
                    .AddChoices(candidates));

            foreach (var service in selected)
            {
                var host = Logger.Prompt(
                    new TextPrompt<string>($"[bold]Enter host for service [blue]{service}[/]: [/]")
                        .PromptStyle("yellow"));

                var tls = Logger.Prompt(
                    new TextPrompt<string>($"[bold]Enter TLS secret for service [blue]{service}[/] (leave blank if none): [/]")
                        .PromptStyle("yellow")
                        .AllowEmpty());
                CurrentState.IngressDefinitions[service] = new IngressDefinition
                {
                    Host = host,
                    Path = "/",
                    TlsSecret = string.IsNullOrWhiteSpace(tls) ? null : tls
                };
            }

            if (Logger.Confirm("Deploy nginx ingress controller if not present?", false) &&
                !string.IsNullOrEmpty(CurrentState.KubeContext))
            {
                await ingressService.EnsureIngressController(CurrentState.KubeContext);
            }
        }

        return true;
    }

    public override void ValidateNonInteractiveState()
    {
        if (CurrentState.WithIngress == true && (CurrentState.IngressDefinitions == null || CurrentState.IngressDefinitions.Count == 0))
        {
            Logger.ValidationFailed("Ingress definitions are required when running non-interactively.");
        }
    }
}
