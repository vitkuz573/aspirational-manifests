using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class KubernetesIngressServiceTests : BaseServiceTests<IKubernetesIngressService>
{
    [Fact]
    public async Task EnsureIngressController_InstallsController_WhenMissing()
    {
        var fileSystem = new MockFileSystem();
        var kubeCtl = Substitute.For<IKubeCtlService>();
        var k8sService = Substitute.For<IKubernetesService>();
        var console = Substitute.For<IAnsiConsole>();
        var k8sClient = Substitute.For<IKubernetes>();
        k8sService.CreateClient("test").Returns(k8sClient);
        k8sClient.CoreV1.ReadNamespaceAsync("ingress-nginx").Returns(Task.FromException<V1Namespace>(new Exception()));

        var sut = new KubernetesIngressService(fileSystem, kubeCtl, k8sService, console);

        await sut.EnsureIngressController("test");

        await kubeCtl.Received().ApplyManifestFile("test", Arg.Any<string>());
    }
}
