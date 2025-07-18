namespace Aspirate.Commands.Actions.Manifests;

public sealed class ApplyManifestsToClusterAction(
    IKubernetesService kubernetesClientService,
    IKubeCtlService kubeCtlService,
    ISecretProvider secretProvider,
    IFileSystem fileSystem,
    IDaprCliService daprCliService,
    IKustomizeService kustomizeService,
    IServiceProvider serviceProvider) : BaseActionWithNonInteractiveValidation(serviceProvider)
{
    public override async Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Handle Deployment to Cluster[/]");

        var secretFiles = new List<string>();
        var manifestsPath = GetManifestsPath();
        var overlayPath = CurrentState.OverlayPath;
        var basePath = CurrentState.InputPath!;

        try
        {
            await kubernetesClientService.InteractivelySelectKubernetesCluster(CurrentState);

            await HandleDapr();

            await kustomizeService.WriteSecretsOutToTempFiles(CurrentState, secretFiles, secretProvider);

            var imagePullSecretFile = await kustomizeService.WriteImagePullSecretToTempFile(CurrentState, secretProvider);

            if (!string.IsNullOrEmpty(imagePullSecretFile))
            {
                secretFiles.Add(imagePullSecretFile);
                await kubeCtlService.ApplyManifestFile(CurrentState.KubeContext, imagePullSecretFile);
            }

            await kubeCtlService.ApplyManifests(CurrentState.KubeContext, basePath);

            if (!string.IsNullOrEmpty(overlayPath))
            {
                await kubeCtlService.ApplyManifests(CurrentState.KubeContext, overlayPath);
            }
            await HandleRollingRestart();
            Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done:[/] Deployments successfully applied to cluster [blue]'{CurrentState.KubeContext}'[/]");

            return true;
        }
        catch (Exception e)
        {
            Logger.MarkupLine("[red](!)[/] Failed to apply manifests to cluster.");
            Logger.MarkupLine($"[red](!)[/] Error: {e.Message}");
            return false;
        }
        finally
        {
            kustomizeService.CleanupSecretEnvFiles(CurrentState.DisableSecrets, secretFiles);
        }
    }

    private async Task HandleDapr()
    {
        var manifestsPath = GetManifestsPath();
        if (!fileSystem.Directory.Exists(fileSystem.Path.Combine(manifestsPath, "dapr")))
        {
            return;
        }

        var daprCliInstalled = daprCliService.IsDaprCliInstalledOnMachine();

        if (!daprCliInstalled)
        {
            Logger.MarkupLine("[yellow]Dapr cli is required to perform dapr installation in your cluster.[/]");
            Logger.MarkupLine("[yellow]Please install dapr cli following the guide here:[blue]https://docs.dapr.io/getting-started/install-dapr-cli/[/][/]");
            Logger.MarkupLine("[yellow]Manifest deployment will continue, but dapr will not be installed by aspirate.[/]");
            return;
        }

        var daprInstalled = await daprCliService.IsDaprInstalledInCluster();

        if (!daprInstalled)
        {
            Logger.MarkupLine("Dapr is required for this workload as you have dapr components, but is not installed in the cluster.");
            Logger.MarkupLine($"Installing Dapr in cluster [blue]'{CurrentState.KubeContext}'[/]");
            var result = await daprCliService.InstallDaprInCluster();

            if (result.ExitCode != 0)
            {
                Logger.MarkupLine($"[red](!)[/] Failed to install Dapr in cluster [blue]'{CurrentState.KubeContext}'[/]");
                Logger.MarkupLine($"[red](!)[/] Error: {result.Error}");
                ActionCausesExitException.ExitNow();
            }

            Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done:[/] Dapr installed in cluster [blue]'{CurrentState.KubeContext}'[/]");
        }
    }

    public override void ValidateNonInteractiveState()
    {
        if (!CurrentState.ActiveKubernetesContextIsSet)
        {
            Logger.ValidationFailed("Cannot apply manifests to cluster without specifying the kubernetes context to use.");
        }

        if (string.IsNullOrEmpty(GetManifestsPath()))
        {
            Logger.ValidationFailed("Cannot apply manifests to cluster without specifying the input path to use for manifests.");
        }
    }

    private async Task HandleRollingRestart()
    {
        if (CurrentState.RollingRestart != true)
        {
            return;
        }

        var result = await kubeCtlService.PerformRollingRestart(CurrentState.KubeContext, GetManifestsPath());

        if (!result)
        {
            Logger.MarkupLine("[red](!)[/] Selected deployment options have failed.");
            ActionCausesExitException.ExitNow();
        }
    }

    private string GetManifestsPath() => !string.IsNullOrEmpty(CurrentState.OverlayPath) ? CurrentState.OverlayPath! : CurrentState.InputPath!;
}
