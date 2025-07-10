using Aspirate.Shared.Models.AspireManifests.Components.Common;
namespace Aspirate.Services.Implementations;

public sealed class ContainerCompositionService(
    IFileSystem filesystem,
    IAnsiConsole console,
    IProjectPropertyService projectPropertyService,
    IShellExecutionService shellExecutionService) : IContainerCompositionService
{
    public async Task<bool> BuildAndPushContainerForProject(
        ProjectResource projectResource,
        MsBuildContainerProperties containerDetails,
        ContainerOptions options,
        bool nonInteractive = false,
        string? runtimeIdentifier = null,
        string? basePath = null)
    {
        await CheckIfBuilderIsRunning(options.ContainerBuilder);

        var fullProjectPath = filesystem.NormalizePath(projectResource.Path, basePath);

        var argumentsBuilder = ArgumentsBuilder.Create();

        if (!string.IsNullOrEmpty(options.Prefix))
        {
            containerDetails.ContainerRepository = $"{options.Prefix}/{containerDetails.ContainerRepository}";
        }

        AddProjectPublishArguments(argumentsBuilder, fullProjectPath, runtimeIdentifier);
        AddContainerDetailsToArguments(argumentsBuilder, containerDetails);

        await shellExecutionService.ExecuteCommand(new()
        {
            Command = DotNetSdkLiterals.DotNetCommand,
            ArgumentsBuilder = argumentsBuilder,
            NonInteractive = nonInteractive,
            OnFailed = HandleBuildErrors,
            ShowOutput = true,
        });

        return true;
    }

    public async Task<bool> BuildAndPushContainerForDockerfile(DockerfileResource dockerfileResource, ContainerOptions options, bool? nonInteractive = false, string? basePath = null) =>
        await BuildAndPushContainerForDockerfile(
            dockerfileResource.Context,
            dockerfileResource.Env,
            dockerfileResource.BuildArgs,
            null,
            dockerfileResource.Path,
            dockerfileResource.Name,
            options,
            nonInteractive,
            basePath);

    public async Task<bool> BuildAndPushContainerForDockerfile(ContainerV1Resource containerV1Resource, ContainerOptions options, bool? nonInteractive = false, string? basePath = null) =>
        await BuildAndPushContainerForDockerfile(
            containerV1Resource.Build.Context,
            containerV1Resource.Env,
            containerV1Resource.Build.Args,
            containerV1Resource.Build.Secrets,
            containerV1Resource.Build.Dockerfile,
            containerV1Resource.Name,
            options,
            nonInteractive,
            basePath);

    private async Task<bool> BuildAndPushContainerForDockerfile(
        string context,
        Dictionary<string, string>? env,
        Dictionary<string, string>? buildArgs,
        Dictionary<string, BuildSecret>? secrets,
        string dockerfile,
        string resourceName,
        ContainerOptions options,
        bool? nonInteractive = false,
        string? basePath = null)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        await CheckIfBuilderIsRunning(options.ContainerBuilder);

        var fullDockerfilePath = filesystem.GetFullPath(dockerfile, basePath);
        var fullContextPath = filesystem.GetFullPath(context, basePath);

        if (secrets is not null)
        {
            foreach (var secret in secrets.Values)
            {
                if (secret.Type == BuildSecretType.File && secret.Source is not null)
                {
                    secret.Source = filesystem.GetFullPath(secret.Source, basePath);
                }
            }
        }

        var fullImages = options.ToImageNames(resourceName);

        var result = await BuildContainer(fullContextPath, env, buildArgs, secrets, options.ContainerBuilder, nonInteractive, fullImages, fullDockerfilePath);

        CheckSuccess(result);

        result = await PushContainer(options.ContainerBuilder, options.Registry, fullImages, nonInteractive);

        CheckSuccess(result);

        return true;
    }

    private async Task<ShellCommandResult> PushContainer(string builder, string? registry, List<string> fullImages, bool? nonInteractive)
    {
        var command = GetContainerBuilderCommand(builder);
        if (!string.IsNullOrEmpty(registry))
        {
            ShellCommandResult? result = null;

            foreach (var fullImage in fullImages)
            {
                var pushArgumentBuilder = ArgumentsBuilder
                    .Create()
                    .AppendArgument(DockerLiterals.PushCommand, string.Empty, quoteValue: false);

                pushArgumentBuilder.AppendArgument(fullImage.ToLower(), string.Empty, quoteValue: false, allowDuplicates: true);

                result = await shellExecutionService.ExecuteCommand(
                    new()
                    {
                        Command = command,
                        ArgumentsBuilder = pushArgumentBuilder,
                        NonInteractive = nonInteractive.GetValueOrDefault(),
                        ShowOutput = true,
                    });

                if (!result.Success)
                {
                    break;
                }
            }

            return result;
        }

        return new ShellCommandResult(true, string.Empty, string.Empty, 0);
    }

    private Task<ShellCommandResult> BuildContainer(
        string context,
        Dictionary<string, string>? env,
        Dictionary<string, string>? buildArgs,
        Dictionary<string, BuildSecret>? secrets,
        string builder,
        bool? nonInteractive,
        List<string> tags,
        string fullDockerfilePath)
    {
        var command = GetContainerBuilderCommand(builder);
        var buildArgumentBuilder = ArgumentsBuilder
            .Create()
            .AppendArgument(DockerLiterals.BuildCommand, string.Empty, quoteValue: false);

        foreach (var tag in tags)
        {
            buildArgumentBuilder.AppendArgument(DockerLiterals.TagArgument, tag.ToLower(), allowDuplicates: true);
        }

        if (env is not null)
        {
            AddDockerBuildArgs(buildArgumentBuilder, env);
        }

        if (buildArgs is not null)
        {
            AddDockerBuildArgs(buildArgumentBuilder, buildArgs);
        }

        List<string>? envVariables = null;
        if (secrets is not null)
        {
            envVariables = ValidateSecrets(secrets).ToList();
            AddDockerSecrets(buildArgumentBuilder, secrets);
        }

        buildArgumentBuilder
            .AppendArgument(DockerLiterals.DockerFileArgument, fullDockerfilePath)
            .AppendArgument(context, string.Empty, quoteValue: false);

        try
        {
            return shellExecutionService.ExecuteCommand(new()
            {
                Command = command,
                ArgumentsBuilder = buildArgumentBuilder,
                NonInteractive = nonInteractive.GetValueOrDefault(),
                ShowOutput = true,
            });
        }
        finally
        {
            if (envVariables is not null)
            {
                foreach (var variable in envVariables)
                {
                    Environment.SetEnvironmentVariable(variable, null);
                }
            }
        }
    }

    private async Task HandleBuildErrors(string command, ArgumentsBuilder argumentsBuilder, bool nonInteractive, string errors)
    {
        if (errors.Contains(DotNetSdkLiterals.DuplicateFileOutputError, StringComparison.OrdinalIgnoreCase))
        {
            await HandleDuplicateFilesInOutput(argumentsBuilder, nonInteractive);
            return;
        }

        if (errors.Contains(DotNetSdkLiterals.UnknownContainerRegistryAddress, StringComparison.OrdinalIgnoreCase))
        {
            console.MarkupLine($"[red bold]{DotNetSdkLiterals.UnknownContainerRegistryAddress}: Unknown container registry address, or container registry address not accessible.[/]");
            ActionCausesExitException.ExitNow(1013);
        }

        ActionCausesExitException.ExitNow();
    }

    private async Task HandleDuplicateFilesInOutput(ArgumentsBuilder argumentsBuilder, bool nonInteractive = false)
    {
        var shouldRetry = AskIfShouldRetryHandlingDuplicateFiles(nonInteractive);
        if (shouldRetry)
        {
            argumentsBuilder.AppendArgument(DotNetSdkLiterals.ErrorOnDuplicatePublishOutputFilesArgument, "false");

            await shellExecutionService.ExecuteCommand(new()
            {
                Command = DotNetSdkLiterals.DotNetCommand,
                ArgumentsBuilder = argumentsBuilder,
                NonInteractive = nonInteractive,
                OnFailed = HandleBuildErrors,
                ShowOutput = true,
            });
            return;
        }

        ActionCausesExitException.ExitNow();
    }

    private bool AskIfShouldRetryHandlingDuplicateFiles(bool nonInteractive)
    {
        if (nonInteractive)
        {
            return true;
        }

        return console.Confirm(
            "[red bold]Implicitly, dotnet publish does not allow duplicate filenames to be output to the artefact directory at build time.Would you like to retry the build explicitly allowing them?[/]");
    }

    private static void AddProjectPublishArguments(ArgumentsBuilder argumentsBuilder, string fullProjectPath, string? runtimeIdentifier)
    {
        var defaultRuntimeIdentifier = GetRuntimeIdentifier();

        argumentsBuilder
            .AppendArgument(DotNetSdkLiterals.PublishArgument, fullProjectPath)
            .AppendArgument(DotNetSdkLiterals.ContainerTargetArgument, string.Empty, quoteValue: false)
            .AppendArgument(DotNetSdkLiterals.VerbosityArgument, DotNetSdkLiterals.DefaultVerbosity)
            .AppendArgument(DotNetSdkLiterals.NoLogoArgument, string.Empty, quoteValue: false);

        argumentsBuilder.AppendArgument(DotNetSdkLiterals.RuntimeIdentifierArgument, string.IsNullOrEmpty(runtimeIdentifier) ? defaultRuntimeIdentifier : runtimeIdentifier);
    }

    private static void AddContainerDetailsToArguments(ArgumentsBuilder argumentsBuilder,
        MsBuildContainerProperties containerDetails)
    {
        if (!string.IsNullOrEmpty(containerDetails.ContainerRegistry))
        {
            argumentsBuilder.AppendArgument(DotNetSdkLiterals.ContainerRegistryArgument, containerDetails.ContainerRegistry);
        }

        if (!string.IsNullOrEmpty(containerDetails.ContainerRepository))
        {
            argumentsBuilder.AppendArgument(DotNetSdkLiterals.ContainerRepositoryArgument, containerDetails.ContainerRepository);
        }

        if (!string.IsNullOrEmpty(containerDetails.ContainerImageName))
        {
            argumentsBuilder.AppendArgument(DotNetSdkLiterals.ContainerImageNameArgument, containerDetails.ContainerImageName);
        }

        if (containerDetails.ContainerImageTag is not null && containerDetails.ContainerImageTag.Contains(';'))
        {
            argumentsBuilder.AppendArgument(DotNetSdkLiterals.ContainerImageTagArguments,
                $"\\\"{containerDetails.ContainerImageTag}\\\"");
            return;
        }

        argumentsBuilder.AppendArgument(DotNetSdkLiterals.ContainerImageTagArgument, containerDetails.ContainerImageTag);
    }

    private static void AddDockerBuildArgs(ArgumentsBuilder argumentsBuilder, Dictionary<string, string> dockerfileEnv)
    {
        foreach (var (key, value) in dockerfileEnv)
        {
            argumentsBuilder.AppendArgument(DockerLiterals.BuildArgArgument, $"{key}=\"{value}\"", quoteValue: false, allowDuplicates: true);
        }
    }

    private IEnumerable<string> ValidateSecrets(Dictionary<string, BuildSecret> secrets)
    {
        List<string> envVars = new();
        foreach (var (key, secret) in secrets)
        {
            switch (secret.Type)
            {
                case BuildSecretType.Env:
                    if (!string.IsNullOrWhiteSpace(secret.Source))
                    {
                        throw new InvalidOperationException($"Build secret '{key}' of type 'env' does not support 'source'.");
                    }
                    if (string.IsNullOrWhiteSpace(secret.Value))
                    {
                        throw new InvalidOperationException($"Build secret '{key}' of type 'env' requires a value.");
                    }

                    Environment.SetEnvironmentVariable(key, secret.Value);
                    envVars.Add(key);
                    break;
                case BuildSecretType.File:
                    if (!string.IsNullOrWhiteSpace(secret.Value))
                    {
                        throw new InvalidOperationException($"Build secret '{key}' of type 'file' does not support 'value'.");
                    }
                    if (string.IsNullOrWhiteSpace(secret.Source))
                    {
                        throw new InvalidOperationException($"Build secret '{key}' of type 'file' requires a source.");
                    }
                    if (!filesystem.File.Exists(secret.Source))
                    {
                        throw new InvalidOperationException($"Build secret '{key}' file '{secret.Source}' does not exist.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Build secret '{key}' has unsupported type '{secret.Type}'.");
            }
        }

        return envVars;
    }

    private static void AddDockerSecrets(ArgumentsBuilder argumentsBuilder, Dictionary<string, BuildSecret> secrets)
    {
        foreach (var (key, secret) in secrets)
        {
            switch (secret.Type)
            {
                case BuildSecretType.File when secret.Source is not null:
                    argumentsBuilder.AppendArgument(DockerLiterals.SecretArgument, $"id={key},src=\"{secret.Source}\"", quoteValue: false, allowDuplicates: true);
                    break;
                case BuildSecretType.Env:
                    argumentsBuilder.AppendArgument(DockerLiterals.SecretArgument, $"id={key},env={key}", quoteValue: false, allowDuplicates: true);
                    break;
            }
        }
    }

    private async Task CheckIfBuilderIsRunning(string builder)
    {
        var command = GetContainerBuilderCommand(builder);
        var builderAvailable = shellExecutionService.IsCommandAvailable(command);

        if (!builderAvailable.IsAvailable)
        {
            console.MarkupLine($"[red bold]{command} is not available or found on your system.[/]");
            ActionCausesExitException.ExitNow();
        }

        var argumentsBuilder = ArgumentsBuilder
            .Create()
            .AppendArgument("info", string.Empty, quoteValue: false)
            .AppendArgument("--format", "json", quoteValue: false);

        var builderCheckResult = await shellExecutionService.ExecuteCommand(new()
        {
            Command = builderAvailable.FullPath,
            ArgumentsBuilder = argumentsBuilder,
        });

        ValidateBuilderOutput(builderCheckResult);
    }

    private void ValidateBuilderOutput(ShellCommandResult builderCheckResult)
    {
        if (builderCheckResult == null)
        {
            throw new InvalidOperationException("Builder check result was null.");
        }

        if (builderCheckResult.Success)
        {
            return;
        }

        var builderInfo = builderCheckResult.Output.TryParseAsJsonDocument();
        if (builderInfo == null || !builderInfo.RootElement.TryGetProperty("ServerErrors", out var errorProperty))
        {
            return;
        }

        if (errorProperty.ValueKind == JsonValueKind.Array && errorProperty.GetArrayLength() == 0)
        {
            return;
        }

        string messages = string.Join(Environment.NewLine, errorProperty.EnumerateArray());
        console.MarkupLine("[red][bold]The daemon server reported errors:[/][/]");
        console.MarkupLine($"[red]{messages}[/]");
        ActionCausesExitException.ExitNow();
    }

    private static void CheckSuccess(ShellCommandResult result)
    {
        if (result.ExitCode != 0)
        {
            ActionCausesExitException.ExitNow(result.ExitCode);
        }
    }

    private static string GetRuntimeIdentifier()
    {
        var architecture = RuntimeInformation.OSArchitecture;

        return architecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
    }

    private static string GetContainerBuilderCommand(string builder)
    {
        return builder.ToLower() switch
        {
            "podman" => ContainerBuilder.Podman.Value,
            "nerdctl" => ContainerBuilder.Nerdctl.Value,
            _ => ContainerBuilder.Docker.Value,
        };
    }
}
