using Aspirate.Processors.Resources.Project;

namespace Aspirate.Commands.Actions.Containers;

public sealed class BuildAndPushContainersFromProjectsAction(
    IServiceProvider serviceProvider) : BaseAction(serviceProvider)
{
    public override async Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Handling Projects[/]");

        if (CurrentState.SkipBuild == true)
        {
            Logger.MarkupLine("[bold]Skipping build and push action as requested.[/]");
            return true;
        }

        if (NoSelectedProjectComponents())
        {
            return true;
        }

        Logger.MarkupLine("[bold]Building all project resources, and pushing containers[/]");

        foreach (var resource in CurrentState.SelectedProjectComponents)
        {
            var projectProcessor = Services.GetRequiredKeyedService<IResourceProcessor>(resource.Value.Type) as BaseProjectProcessor;

            await projectProcessor.BuildAndPushProjectContainer(resource, new()
            {
                ContainerBuilder = CurrentState.ContainerBuilder.ToLower(),
                Prefix = CurrentState.ContainerRepositoryPrefix,
                Registry = CurrentState.ContainerRegistry,
                BuildArgs = CurrentState.ContainerBuildArgs?.ToDictionary(arg => arg.Split('=')[0], arg => arg.Split('=')[1]),
                BuildContext = CurrentState.ContainerBuildContext,
                Tags = CurrentState.ContainerImageTags
            }, CurrentState.NonInteractive, CurrentState.RuntimeIdentifier, CurrentState.PreferDockerfile, CurrentState.ManifestDirectory);
        }

        Logger.MarkupLine("[bold]Building and push completed for all selected project components.[/]");

        return true;
    }

    private bool NoSelectedProjectComponents()
    {
        if (CurrentState.SelectedProjectComponents.Count != 0)
        {
            return false;
        }

        Logger.MarkupLine("[bold]No project components selected. Skipping build and publish action.[/]");
        return true;
    }
}
