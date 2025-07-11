namespace Aspirate.Services.Implementations;

public class KubernetesIngressService(IFileSystem fileSystem, IKubeCtlService kubeCtlService, IKubernetesService kubernetesService, IAnsiConsole logger) : IKubernetesIngressService
{
    private static readonly Dictionary<string, (string Namespace, string Url)> ControllerData = new()
    {
        {
            IngressController.Nginx.Value,
            ("ingress-nginx", "https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml")
        }
    };

    public async Task EnsureIngressController(string context, string controller)
    {
        var client = kubernetesService.CreateClient(context);
        if (!ControllerData.TryGetValue(controller, out var data))
        {
            logger.MarkupLine($"[red]Unknown ingress controller '{controller}'.[/]");
            return;
        }
        try
        {
            _ = await client.CoreV1.ReadNamespaceAsync(data.Namespace);
            logger.MarkupLine("[yellow]Ingress controller already installed.[/]");
            return;
        }
        catch
        {
            // ignore if not found
        }

        var tempFile = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), $"ingress-{Path.GetRandomFileName()}.yaml");
        using var httpClient = new HttpClient();
        var manifest = await httpClient.GetStringAsync(data.Url);
        await fileSystem.File.WriteAllTextAsync(tempFile, manifest);

        await kubeCtlService.ApplyManifestFile(context, tempFile);

        fileSystem.File.Delete(tempFile);
    }
}
