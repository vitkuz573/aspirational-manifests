namespace Aspirate.Commands.Actions.Manifests;

using Aspirate.Shared.Interfaces.Services;
using Aspirate.Shared.Literals;
using Aspirate.Shared.Models.Aspirate;
using System.IO.Abstractions;
using System.IO;
using System.Linq;

public sealed class GenerateFinalKustomizeManifestAction(
    IAspireManifestCompositionService manifestCompositionService,
    IManifestWriter manifestWriter,
    IFileSystem fileSystem,
    IServiceProvider serviceProvider) : BaseAction(serviceProvider)
{
    public override Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Handling Final Manifest[/]");

        if (CurrentState.SkipFinalKustomizeGeneration == true)
        {
            Logger.MarkupLine("[blue]Skipping final manifest generation as requested.[/]");
            return Task.FromResult(true);
        }

        if (NoSupportedComponentsExitAction())
        {
            return Task.FromResult(true);
        }

        if (!CurrentState.NonInteractive)
        {
            if (!ShouldCreateFinalManifest())
            {
                return Task.FromResult(true);
            }
        }

        var manifests = CurrentState.FinalResources.Select(x => x.Key).ToList();

        var templateDataBuilder = new KubernetesDeploymentData()
            .SetNamespace(CurrentState.Namespace)
            .SetWithPrivateRegistry(CurrentState.WithPrivateRegistry.GetValueOrDefault());

        HandleNamespace(CurrentState.OutputPath, CurrentState.TemplatePath, CurrentState.Namespace, templateDataBuilder, manifests);
        HandlePrivateRegistry(CurrentState.WithPrivateRegistry);
        HandleDapr(CurrentState.OutputPath, manifests);
        HandleDashboard(CurrentState.IncludeDashboard, CurrentState.OutputPath, CurrentState.TemplatePath, templateDataBuilder, manifests);

        Logger.MarkupLine($"[bold]Generating final manifest with name [blue]'{TemplateLiterals.ComponentKustomizeType}.yaml'[/][/]");

        var templateData = templateDataBuilder.SetManifests(manifests);

        manifestWriter.CreateComponentKustomizeManifest(CurrentState.OutputPath, templateData, CurrentState.TemplatePath);

        Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Generating [blue]{CurrentState.OutputPath}/{TemplateLiterals.ComponentKustomizeType}.yaml[/]");

        return Task.FromResult(true);
    }

    private void HandlePrivateRegistry(bool? withPrivateRegistry)
    {
        if (!withPrivateRegistry.GetValueOrDefault())
        {
            return;
        }

        Logger.MarkupLine("[bold]Private registry detected. Image pull secret will be generated dynamically during deployment.[/]");
    }

    private void HandleNamespace(string outputPath, string? templatePath, string? @namespace, KubernetesDeploymentData templateDataBuilder, List<string> manifests)
    {
        if (string.IsNullOrEmpty(@namespace))
        {
            return;
        }

        Logger.MarkupLine($"[bold]Generating namespace manifest with name [blue]'{@namespace}'[/][/]");
        manifestWriter.CreateNamespace(outputPath, templateDataBuilder, templatePath);
        manifests.Add($"{TemplateLiterals.NamespaceType}.yaml");
        Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Generating [blue]{outputPath}/{TemplateLiterals.NamespaceType}.yaml[/]");
    }

    private void HandleDashboard(bool? withDashboard, string outputPath, string? templatePath, KubernetesDeploymentData templateDataBuilder, List<string> manifests)
    {
        if (withDashboard == false)
        {
            return;
        }

        templateDataBuilder = templateDataBuilder.SetContainerImage(AspireLiterals.DashboardImage);

        Logger.MarkupLine("[bold]Generating Aspire Dashboard manifest[/]");
        manifestWriter.CreateDashboard(outputPath, templateDataBuilder, templatePath);
        manifests.Add($"{TemplateLiterals.DashboardType}.yaml");
        Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Generating [blue]{outputPath}/{TemplateLiterals.DashboardType}.yaml[/]");
    }

    private void HandleDapr(string outputPath, List<string> manifests)
    {
        if (!fileSystem.Directory.Exists(fileSystem.Path.Combine(outputPath, "dapr")))
        {
            return;
        }

        var daprFiles = fileSystem.Directory.GetFiles(fileSystem.Path.Combine(outputPath, "dapr"), "*.yaml", SearchOption.AllDirectories);
        manifests.AddRange(daprFiles.Select(daprFile => daprFile.Replace(outputPath, string.Empty).TrimStart(Path.DirectorySeparatorChar)));
    }

    private bool ShouldCreateFinalManifest()
    {
        if (CurrentState.SkipFinalKustomizeGeneration == false)
        {
            return true;
        }

        var shouldGenerateFinalKustomizeManifest = Logger.Confirm(
            "[bold]Would you like to generate the top level kustomize manifest to run against your kubernetes cluster?[/]");

        CurrentState.SkipFinalKustomizeGeneration = !shouldGenerateFinalKustomizeManifest;

        if (!shouldGenerateFinalKustomizeManifest)
        {
            Logger.MarkupLine("[yellow](!)[/] Skipping final manifest");
            return false;
        }

        return true;
    }

    private bool NoSupportedComponentsExitAction()
    {
        if (CurrentState.HasSelectedSupportedComponents)
        {
            return false;
        }

        Logger.MarkupLine("[bold]No supported components selected. Final manifest does not need to be generated as it would be empty.[/]");
        return true;
    }
}
