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

    public async Task<string> RenderManifestUsingKustomize(string kustomizePath)
    {
        var arguments = new ArgumentsBuilder()
            .AppendArgument("build", kustomizePath);

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

        if (!secretProvider.SecretStateExists(state))
        {
            return;
        }


        if (state.SecretState is null || state.SecretState.Secrets.Count == 0)
        {
            return;
        }

        var basePath = !string.IsNullOrEmpty(state.OverlayPath)
            ? state.OverlayPath!
            : !string.IsNullOrEmpty(state.InputPath)
                ? state.InputPath!
                : fileSystem.Path.GetTempPath();

        foreach (var resourceSecrets in secretProvider.State.Secrets.Where(x => x.Value.Keys.Count > 0))
        {
            var resourcePath = fileSystem.Path.Combine(basePath, resourceSecrets.Key);

            if (!fileSystem.Directory.Exists(resourcePath))
            {
                continue;
            }

            var secretFile = fileSystem.Path.Combine(resourcePath, $".{resourceSecrets.Key}.secrets");

            files.Add(secretFile);

            await using var streamWriter = fileSystem.File.CreateText(secretFile);

            foreach (var key in resourceSecrets.Value.Keys)
            {
                var secretValue = secretProvider.GetSecret(resourceSecrets.Key, key);

                await streamWriter.WriteLineAsync($"{key}={secretValue}");
            }

            await streamWriter.FlushAsync();

            streamWriter.Close();

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
        }
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
