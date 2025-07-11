using System.Collections.Generic;
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
            .SetIngressClassName(IngressController.Nginx.Value)
            .SetIngressHosts(["example.com"])
            .SetIngressPath("/")
            .SetIngressTlsSecret("tls")
            .SetPorts(new List<Ports> { new Ports { Name = "http", InternalPort = 8080 } })
            .SetIngressAnnotations(new Dictionary<string, string> { { "test", "value" } });

        var ingress = data.ToKubernetesIngress();

        ingress.Spec.Rules.First().Host.Should().Be("example.com");
        ingress.Spec.Tls.First().SecretName.Should().Be("tls");
        ingress.Metadata.Annotations.Should().ContainKey("test").WhoseValue.Should().Be("value");
        ingress.Spec.IngressClassName.Should().Be(IngressController.Nginx.Value);
    }

    [Fact]
    public void ToKubernetesObjects_IncludesIngress_WhenEnabled()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("test")
            .SetIngressEnabled(true)
            .SetIngressClassName(IngressController.Nginx.Value)
            .SetIngressHosts(["example.com"])
            .SetIngressPath("/")
            .SetPorts(new List<Ports> { new Ports { Name = "http", InternalPort = 8080 } });

        var objects = data.ToKubernetesObjects();

        objects.OfType<V1Ingress>().Should().ContainSingle();
    }

    [Fact]
    public void ToKubernetesIngress_UsesBindingPort_WhenDifferentFromTargetPort()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("test")
            .SetIngressEnabled(true)
            .SetIngressClassName(IngressController.Nginx.Value)
            .SetIngressHosts(["example.com"])
            .SetIngressPath("/")
            .SetPorts(new List<Ports> { new Ports { Name = "http", InternalPort = 8080, ExternalPort = 80 } });

        var ingress = data.ToKubernetesIngress();

        ingress.Spec.Rules.First().Http.Paths.First().Backend.Service.Port.Number.Should().Be(80);
    }
}
