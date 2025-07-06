namespace Aspirate.Shared.Extensions;

public static class PathExtensions
{
    public static string NormalizePath(this IFileSystem fileSystem, string pathToTarget, string? basePath = null)
    {
        if (string.IsNullOrEmpty(pathToTarget))
        {
            return basePath ?? fileSystem.Directory.GetCurrentDirectory();
        }

        if (fileSystem.Path.IsPathRooted(pathToTarget))
        {
            return pathToTarget;
        }

        var normalizedProjectPath = pathToTarget.Replace('\\', fileSystem.Path.DirectorySeparatorChar);

        var root = basePath ?? fileSystem.Directory.GetCurrentDirectory();

        return fileSystem.Path.Combine(root, normalizedProjectPath);
    }

    public static string GetFullPath(this IFileSystem fileSystem, string path, string? basePath = null)
    {
        if (fileSystem.Path.IsPathRooted(path))
        {
            return fileSystem.Path.GetFullPath(path);
        }

        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.StartsWith($"~{fileSystem.Path.DirectorySeparatorChar}"))
        {
            return fileSystem.Path.Combine(homePath, path.TrimStart('~', fileSystem.Path.DirectorySeparatorChar));
        }

        var root = basePath ?? fileSystem.Directory.GetCurrentDirectory();
        return fileSystem.Path.GetFullPath(fileSystem.Path.Combine(root, path));
    }

    public static string AspirateAppDataFolder(this IFileSystem fileSystem)
    {
        var appDataFolder = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AspirateLiterals.AppDataFolder);

        if (!Directory.Exists(appDataFolder))
        {
            fileSystem.Directory.CreateDirectory(appDataFolder);
        }

        return appDataFolder;
    }
}
