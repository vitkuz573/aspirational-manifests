using System.Security.AccessControl;
using System.Security.Principal;

namespace Aspirate.Services.Implementations;

public class KustomizeService(IFileSystem fileSystem, IShellExecutionService shellExecutionService, IAnsiConsole logger, IManifestWriter manifestWriter) : IKustomizeService
{
    public CommandAvailableResult IsKustomizeAvailable()
    {
        var isKustomizeAvailable = shellExecutionService.IsCommandAvailable("kustomize");

        if (!isKustomizeAvailable.IsAvailable)
        {
            throw new InvalidOperationException("Kustomize is not installed. Please install Kustomize to create a Helm chart.");
        }

        return isKustomizeAvailable;
    }

    public async Task<string> RenderManifestUsingKustomize(string kustomizePath, string? overlayPath = null)
    {
        var buildPath = !string.IsNullOrEmpty(overlayPath) ? overlayPath : kustomizePath;

        var arguments = new ArgumentsBuilder()
            .AppendArgument("build", buildPath);

        var result = await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = "kustomize",
            ArgumentsBuilder = arguments,
            ShowOutput = false,
        });

        if (!result.Success)
        {
            throw new InvalidOperationException("Failed to render manifest using kustomize.");
        }

        return result.Output;
    }

    public async Task WriteSecretsOutToTempFiles(AspirateState state, List<string> files, ISecretProvider secretProvider)
    {
        if (state.DisableSecrets == true)
        {
            return;
        }

        var startPath = !string.IsNullOrEmpty(state.OverlayPath)
            ? state.OverlayPath
            : !string.IsNullOrEmpty(state.InputPath)
                ? state.InputPath!
                : fileSystem.Path.GetTempPath();

        if (!fileSystem.Directory.Exists(startPath))
        {
            logger.MarkupLine($"[yellow]Overlay directory '{startPath}' does not exist.[/]");
            return;
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await ProcessKustomizationDirectory(startPath, visited, files, secretProvider);
    }

    private async Task ProcessKustomizationDirectory(string directory, HashSet<string> visited, List<string> files, ISecretProvider secretProvider)
    {
        var fullPath = fileSystem.Path.GetFullPath(directory);
        if (!visited.Add(fullPath))
        {
            return;
        }

        var kustomizationFile = GetKustomizationFilePath(fullPath);
        if (kustomizationFile is null)
        {
            return;
        }

        KustomizationYaml? kustomization = null;
        try
        {
            var text = await fileSystem.File.ReadAllTextAsync(kustomizationFile);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            kustomization = deserializer.Deserialize<KustomizationYaml>(text);
        }
        catch (Exception ex)
        {
            logger.MarkupLine($"[yellow]Failed to parse kustomization at '{kustomizationFile}': {ex.Message}[/]");
            return;
        }

        if (kustomization is null)
        {
            return;
        }

        if (kustomization.SecretGenerator is not null)
        {
            foreach (var generator in kustomization.SecretGenerator)
            {
                if (generator.Envs is null)
                {
                    continue;
                }

                foreach (var env in generator.Envs)
                {
                    var envPath = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(fullPath, env));
                    var resourceName = ExtractResourceName(env);

                    if (!fileSystem.File.Exists(envPath))
                    {
                        files.Add(envPath);
                        var stream = fileSystem.File.Create(envPath);
                        stream.Close();
                    }
                    else
                    {
                        files.Add(envPath);
                    }

                    if (secretProvider.State?.Secrets != null
                        && secretProvider.State.Secrets.TryGetValue(resourceName, out var secrets)
                        && secrets.Count > 0)
                    {
                        await using var writer = fileSystem.File.CreateText(envPath);
                        foreach (var key in secrets.Keys)
                        {
                            var secretValue = secretProvider.GetSecret(resourceName, key);
                            await writer.WriteLineAsync($"{key}={secretValue}");
                        }

                        await writer.FlushAsync();
                        writer.Close();

                        if (OperatingSystem.IsWindows())
                        {
                            var fileInfo = fileSystem.FileInfo.New(envPath);
                            var security = fileInfo.GetAccessControl();
                            var currentUser = WindowsIdentity.GetCurrent().User;

                            if (currentUser != null)
                            {
                                var rule = new FileSystemAccessRule(currentUser, FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow);

                                security.SetAccessRule(rule);
                                security.SetAccessRuleProtection(true, false);

                                fileInfo.SetAccessControl(security);
                            }
                        }
                        else
                        {
                            try
                            {
                                fileSystem.File.SetUnixFileMode(envPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                            }
                            catch (PlatformNotSupportedException)
                            {
                                // Ignore on platforms that do not support setting file permissions
                            }
                            catch (NotImplementedException)
                            {
                                // Mock file system used in tests does not implement this API
                            }
                        }
                    }
                }
            }
        }

        if (kustomization.Resources is null)
        {
            return;
        }

        foreach (var resource in kustomization.Resources)
        {
            var next = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(fullPath, resource));
            if (fileSystem.Directory.Exists(next) && GetKustomizationFilePath(next) != null)
            {
                await ProcessKustomizationDirectory(next, visited, files, secretProvider);
            }
        }
    }

    private string? GetKustomizationFilePath(string directory)
    {
        foreach (var fileName in ["kustomization.yaml", "kustomization.yml", "Kustomization"])
        {
            var candidate = fileSystem.Path.Combine(directory, fileName);
            if (fileSystem.File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string ExtractResourceName(string envFile)
    {
        var name = Path.GetFileNameWithoutExtension(envFile);
        if (name.StartsWith('.'))
        {
            name = name[1..];
        }

        return name;
    }

    public async Task<string?> WriteImagePullSecretToTempFile(AspirateState state, ISecretProvider secretProvider)
    {
        if (state.WithPrivateRegistry != true)
        {
            return null;
        }

        if (!secretProvider.SecretStateExists(state))
        {
            return null;
        }

        if (secretProvider.State?.Secrets == null || secretProvider.State.Secrets.Count == 0)
        {
            return null;
        }

        const string resourceName = TemplateLiterals.ImagePullSecretType;

        if (!secretProvider.ResourceExists(resourceName))
        {
            return null;
        }

        var registryUrl = secretProvider.GetSecret(resourceName, "registryUrl");
        var registryUsername = secretProvider.GetSecret(resourceName, "registryUsername");
        var registryPassword = secretProvider.GetSecret(resourceName, "registryPassword");
        var registryEmail = secretProvider.GetSecret(resourceName, "registryEmail") ?? string.Empty;

        if (registryUrl == null || registryUsername == null || registryPassword == null)
        {
            return null;
        }

        var secretYaml = manifestWriter.CreateImagePullSecretYaml(registryUrl, registryUsername, registryPassword, registryEmail, resourceName);

        var tempPath = fileSystem.Path.GetTempPath();
        var secretFile = fileSystem.Path.Combine(tempPath, $"{resourceName}.{Path.GetRandomFileName()}.yaml");

        await fileSystem.File.WriteAllTextAsync(secretFile, secretYaml);

        if (OperatingSystem.IsWindows())
        {
            var fileInfo = fileSystem.FileInfo.New(secretFile);
            var security = fileInfo.GetAccessControl();
            var currentUser = WindowsIdentity.GetCurrent().User;

            if (currentUser != null)
            {
                var rule = new FileSystemAccessRule(currentUser, FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow);

                security.SetAccessRule(rule);
                security.SetAccessRuleProtection(true, false);

                fileInfo.SetAccessControl(security);
            }
        }
        else
        {
            try
            {
                fileSystem.File.SetUnixFileMode(secretFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            catch (PlatformNotSupportedException)
            {
                // Ignore on platforms that do not support setting file permissions
            }
            catch (NotImplementedException)
            {
                // Mock file system used in tests does not implement this API
            }
        }

        return secretFile;
    }

public void CleanupSecretEnvFiles(bool? disableSecrets, IEnumerable<string> secretFiles)
    {
        if (disableSecrets == true)
        {
            return;
        }

        foreach (var secretFile in secretFiles.Where(fileSystem.File.Exists))
        {
            fileSystem.File.Delete(secretFile);
        }
    }
}

internal sealed class KustomizationYaml
{
    [YamlMember(Alias = "resources")]
    public List<string>? Resources { get; set; }

    [YamlMember(Alias = "secretGenerator")]
    public List<KustomizationSecretGenerator>? SecretGenerator { get; set; }
}

internal sealed class KustomizationSecretGenerator
{
    [YamlMember(Alias = "envs")]
    public List<string>? Envs { get; set; }
}
