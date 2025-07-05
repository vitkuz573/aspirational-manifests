namespace Aspirate.Processors.Resources.Executable;

public class ExecutableProcessor(
    IFileSystem fileSystem,
    IAnsiConsole console,
    IManifestWriter manifestWriter)
    : BaseResourceProcessor(fileSystem, console, manifestWriter)
{
    public override string ResourceType => AspireComponentLiterals.Executable;

    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<ExecutableResource>(ref reader);

    private static void ValidateExecutable(ExecutableResource? exec, string name)
    {
        if (exec == null)
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Executable} {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(exec.Command))
        {
            throw new InvalidOperationException($"{AspireComponentLiterals.Executable} {name} missing required property 'command'.");
        }
    }

    public override Task<bool> CreateManifests(CreateManifestsOptions options) =>
        // Executable resources do not generate additional manifests.
        Task.FromResult(true);

    public override ComposeService CreateComposeEntry(CreateComposeEntryOptions options)
    {
        var exec = options.Resource.Value as ExecutableResource;
        ValidateExecutable(exec, options.Resource.Key);

        var commands = new List<string>();
        if (!string.IsNullOrEmpty(exec?.Command))
        {
            commands.Add(exec.Command);
        }
        if (exec?.Args != null)
        {
            commands.AddRange(exec.Args);
        }

        var serviceBuilder = Builder.MakeService(options.Resource.Key)
            .WithImage("busybox:latest")
            .WithEnvironment(options.Resource.MapResourceToEnvVars(options.WithDashboard))
            .WithContainerName(options.Resource.Key)
            .WithPortMappings(options.Resource.MapBindingsToPorts().MapPortsToDockerComposePorts())
            .WithCommands(commands.ToArray())
            .WithRestartPolicy(ERestartMode.UnlessStopped);

        var service = serviceBuilder.Build();

        return new ComposeService { Service = service };
    }

    public override List<object> CreateKubernetesObjects(CreateKubernetesObjectsOptions options)
    {
        var exec = options.Resource.Value as ExecutableResource;
        ValidateExecutable(exec, options.Resource.Key);

        var data = new KubernetesDeploymentData()
            .SetWithDashboard(options.WithDashboard.GetValueOrDefault())
            .SetName(options.Resource.Key)
            .SetContainerImage("busybox:latest")
            .SetImagePullPolicy(options.ImagePullPolicy)
            .SetEnv(GetFilteredEnvironmentalVariables(options.Resource, options.DisableSecrets, options.WithDashboard))
            .SetArgs(exec?.Args)
            .SetEntrypoint(exec?.Command)
            .SetPorts(options.Resource.MapBindingsToPorts())
            .SetManifests([])
            .SetWithPrivateRegistry(options.WithPrivateRegistry.GetValueOrDefault())
            .ApplyIngress(options)
            .Validate();

        return data.ToKubernetesObjects(options.EncodeSecrets);
    }
}
