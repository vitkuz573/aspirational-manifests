namespace Aspirate.Services.Implementations;

using System.Security.AccessControl;
using System.Security.Principal;

public class KustomizeService(IFileSystem fileSystem, IShellExecutionService shellExecutionService, IAnsiConsole logger) : IKustomizeService
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

        var tempPath = fileSystem.Path.GetTempPath();

        foreach (var resourceSecrets in secretProvider.State.Secrets.Where(x => x.Value.Keys.Count > 0))
        {
            var secretFile = fileSystem.Path.Combine(tempPath, $"{resourceSecrets.Key}.{Path.GetRandomFileName()}.secrets");

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
                    var rule = new FileSystemAccessRule(currentUser,
                        FileSystemRights.Read | FileSystemRights.Write,
                        AccessControlType.Allow);
                    security.SetAccessRule(rule);
                    security.SetAccessRuleProtection(true, false);
                    fileInfo.SetAccessControl(security);
                }
            }
            else
            {
                fileSystem.File.SetUnixFileMode(secretFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
    }

    public void CleanupSecretEnvFiles(bool? disableSecrets, IEnumerable<string> secretFiles)
    {
        if (disableSecrets == true)
        {
            return;
        }

        foreach (var secretFile in secretFiles.Where(secretFile => fileSystem.File.Exists(secretFile)))
        {
            fileSystem.File.Delete(secretFile);
        }
    }
}
