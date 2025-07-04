using Xunit;

namespace Aspirate.Tests.ExtensionTests;

public class KubernetesIngressTests
{
    [Fact]
    public void ToKubernetesIngress_ShouldReturnExpectedValues()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("test")
            .SetIngressEnabled(true)
            .SetIngressHost("example.com")
            .SetIngressPath("/")
            .SetIngressTlsSecret("tls")
            .SetPorts(new List<Ports> { new Ports { Name = "http", InternalPort = 8080 } });

        var ingress = data.ToKubernetesIngress();

        ingress.Spec.Rules.First().Host.Should().Be("example.com");
        ingress.Spec.Tls.First().SecretName.Should().Be("tls");
    }

    [Fact]
    public void ToKubernetesObjects_IncludesIngress_WhenEnabled()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("test")
            .SetIngressEnabled(true)
            .SetIngressHost("example.com")
            .SetIngressPath("/")
            .SetPorts(new List<Ports> { new Ports { Name = "http", InternalPort = 8080 } });

        var objects = data.ToKubernetesObjects();

        objects.OfType<V1Ingress>().Should().ContainSingle();
    }
}
