namespace Aspirate.Services.Implementations;

public class KubernetesIngressService(IFileSystem fileSystem, IKubeCtlService kubeCtlService, IKubernetesService kubernetesService, IAnsiConsole logger) : IKubernetesIngressService
{
    private const string IngressNamespace = "ingress-nginx";
    private const string IngressManifestUrl = "https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml";

    public async Task EnsureIngressController(string context)
    {
        var client = kubernetesService.CreateClient(context);
        try
        {
            _ = await client.CoreV1.ReadNamespaceAsync(IngressNamespace);
            logger.MarkupLine("[yellow]Ingress controller already installed.[/]");
            return;
        }
        catch
        {
            // ignore if not found
        }

        var tempFile = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), $"ingress-{Path.GetRandomFileName()}.yaml");
        using var httpClient = new HttpClient();
        var manifest = await httpClient.GetStringAsync(IngressManifestUrl);
        await fileSystem.File.WriteAllTextAsync(tempFile, manifest);

        await kubeCtlService.ApplyManifestFile(context, tempFile);

        fileSystem.File.Delete(tempFile);
    }
}
