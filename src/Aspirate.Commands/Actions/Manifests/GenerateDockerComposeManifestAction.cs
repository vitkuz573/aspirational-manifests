using DockerComposeBuilder.Converters;
using DockerComposeBuilder.Emitters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Volume = DockerComposeBuilder.Model.Volume;

namespace Aspirate.Commands.Actions.Manifests;

public sealed class GenerateDockerComposeManifestAction(IServiceProvider serviceProvider, IFileSystem fileSystem) : BaseAction(serviceProvider)
{
    private int _servicePort = 10000;

    public override Task<bool> ExecuteAsync()
    {
        Logger.WriteRuler("[purple]Handling Docker Compose generation[/]");

        var outputFormat = OutputFormat.FromValue(CurrentState.OutputFormat);

        if (outputFormat == OutputFormat.Kustomize)
        {
            Logger.MarkupLine($"[red](!)[/] The output format '{CurrentState.OutputFormat}' is not supported for this action.");
            Logger.MarkupLine($"[red](!)[/] Please use the output format 'compose' instead.");
            ActionCausesExitException.ExitNow();
        }

        var outputPath = GetOutputPath();

        Logger.MarkupLine($"[bold]Generating docker compose file: [blue]'{outputPath}/docker-compose.yaml'[/][/]");

        var services = new List<Service>();

        foreach (var resource in CurrentState.AllSelectedSupportedComponents)
        {
            ProcessIndividualComponent(resource, services);
        }

        if (CurrentState.IncludeDashboard.GetValueOrDefault())
        {
            AddAspireDashboardToCompose(services);
        }

        WriteFile(services, outputPath);

        Logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Generating [blue]{outputPath}/docker-compose.yaml[/]");

        return Task.FromResult(true);
    }

    private void WriteFile(List<Service> services, string outputPath)
    {
        var volumes = CreateVolumes(services);

        var composeFile = Builder.MakeCompose()
            .WithServices(services.ToArray())
            .WithVolumes(volumes.ToArray())
            .Build();

        composeFile.Version = null;

        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlValueCollectionConverter())
            .WithTypeConverter(new PublishedPortConverter())
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithEventEmitter(nextEmitter => new FlowStyleStringSequences(nextEmitter))
            .WithEventEmitter(nextEmitter => new FlowStringEnumConverter(nextEmitter))
            .WithEventEmitter(nextEmitter => new ForceQuotedStringValuesEventEmitter(nextEmitter))
            .WithEmissionPhaseObjectGraphVisitor(args => new YamlIEnumerableSkipEmptyObjectGraphVisitor(args.InnerVisitor))
            .WithNewLine("\n")
            .Build();

        var composeFileString = serializer.Serialize(composeFile);

        WriteComposeOutputToOutputPath(outputPath, composeFileString);
    }

    private void WriteComposeOutputToOutputPath(string outputPath, string composeFileString)
    {
        if (!fileSystem.Directory.Exists(outputPath))
        {
            fileSystem.Directory.CreateDirectory(outputPath);
        }

        fileSystem.File.WriteAllText(fileSystem.Path.Combine(outputPath, "docker-compose.yaml"), composeFileString);
    }

    private static List<Volume> CreateVolumes(List<Service> services)
    {
        var volumes = new List<Volume>();

        foreach (var service in services)
        {
            if (service.Volumes is not null)
            {
                volumes.AddRange(service.Volumes.Select(volume => new Volume { Name = volume.Split(':')[0] }));
            }
        }

        return volumes;
    }

    private void ProcessIndividualComponent(KeyValuePair<string, Resource> resource, List<Service> services)
    {
        if (CurrentState.IsNotDeployable(resource.Value))
        {
            return;
        }

        var handler = Services.GetKeyedService<IResourceProcessor>(resource.Value.Type);

        if (handler is null)
        {
            Logger.MarkupLine($"[yellow]Skipping resource '{resource.Key}' as its type is unsupported.[/]");
            return;
        }

        var response = handler.CreateComposeEntry(new()
        {
            Resource = resource,
            WithDashboard = CurrentState.IncludeDashboard,
            ComposeBuilds = CurrentState.ComposeBuilds?.Any(x=> x == resource.Key) ?? false,
            CurrentState = CurrentState
        });

        if (response.IsProject)
        {
            foreach (var port in response.Service.Ports)
            {
                port.Published = _servicePort;
                _servicePort++;
            }
        }

        services.Add(response.Service);
    }

    private static void AddAspireDashboardToCompose(List<Service> services)
    {
        var environment = new Dictionary<string, string?> { { "DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true" } };

        var ports = new List<Port>
        {
            new() { Published = 18888, Target = 18888 }
        };

        var aspireDashboard = Builder.MakeService(AspireLiterals.DashboardName)
            .WithImage(AspireLiterals.DashboardImage)
            .WithEnvironment(environment)
            .WithContainerName(AspireLiterals.DashboardName)
            .WithRestartPolicy(ERestartMode.UnlessStopped)
            .WithPortMappings(ports.ToArray())
            .Build();

        services.Insert(0, aspireDashboard);
    }

    private string GetOutputPath() =>
        !string.IsNullOrEmpty(CurrentState.OutputPath) ?
            fileSystem.GetFullPath(CurrentState.OutputPath) :
            fileSystem.GetFullPath(AspirateLiterals.DefaultArtifactsPath);
}
