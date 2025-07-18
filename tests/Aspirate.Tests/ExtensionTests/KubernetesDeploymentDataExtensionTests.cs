using System.Collections.Generic;
using System.Linq;
using k8s;
using k8s.Models;
using Xunit;

namespace Aspirate.Tests.ExtensionTests;

public class KubernetesDeploymentDataExtensionTests
{
    [Fact]
    public void ToKubernetesLabels_ShouldReturnCorrectLabels()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test");

        // Act
        var result = data.ToKubernetesLabels();

        // Assert
        result.Should().ContainKey("app");
        result["app"].Should().Be("test");
    }

    [Fact]
    public void ToKubernetesObjectMetaData_ShouldReturnCorrectMetadata()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetNamespace("namespace");

        // Act
        var result = data.ToKubernetesObjectMetaData();

        // Assert
        result.Name.Should().Be("test");
        result.NamespaceProperty.Should().Be("namespace");
    }

    [Fact]
    public void ToKubernetesConfigMap_ShouldReturnCorrectConfigMap()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetEnv(new Dictionary<string, string?> { { "key", "value" } });

        // Act
        var result = data.ToKubernetesConfigMap();

        // Assert
        result.Data.Should().ContainKey("key");
        result.Data["key"].Should().Be("value");
    }

    [Fact]
    public void ToKubernetesSecret_ShouldReturnCorrectSecret()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetSecrets(new Dictionary<string, string?> { { "key", "value" } });

        // Act
        var result = data.ToKubernetesSecret();

        // Assert
        result.Data.Should().ContainKey("key");
        result.Data["key"].Should().NotBeEmpty().And.BeEquivalentTo("dmFsdWU="u8.ToArray());
    }

    [Fact]
    public void ToKubernetesContainer_ShouldReturnCorrectContainer()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image");

        // Act
        var result = data.ToKubernetesContainer();

        // Assert
        result.Name.Should().Be("test");
        result.Image.Should().Be("test-image");
    }

    [Fact]
    public void ToKubernetesDeployment_ShouldReturnCorrectDeployment()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image");

        // Act
        var result = data.ToKubernetesDeployment();

        // Assert
        result.Spec.Template.Spec.Containers[0].Name.Should().Be("test");
        result.Spec.Template.Spec.Containers[0].Image.Should().Be("test-image");
    }

    [Fact]
    public void ToKubernetesService_ShouldReturnCorrectService()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetPorts(new List<Ports> { new Ports { Name = "test-port", InternalPort = 8080, ExternalPort = 8080 } });

        // Act
        var result = data.ToKubernetesService();

        // Assert
        result.Spec.Ports[0].Name.Should().Be("test-port");
        result.Spec.Ports[0].Port.Should().Be(8080);
        result.Spec.Ports[0].TargetPort.Value.Should().Be("8080");
    }

    [Fact]
    public void ToKubernetesService_DifferentPorts_ShouldMapExternalAndInternal()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetPorts(new List<Ports> { new Ports { Name = "test-port", InternalPort = 8080, ExternalPort = 80 } });

        // Act
        var result = data.ToKubernetesService();

        // Assert
        result.Spec.Ports[0].Name.Should().Be("test-port");
        result.Spec.Ports[0].Port.Should().Be(80);
        result.Spec.Ports[0].TargetPort.Value.Should().Be("8080");
    }

    [Fact]
    public void ToKubernetesObjects_ShouldReturnCorrectObjects()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image")
            .SetPorts(new List<Ports> { new Ports { Name = "test-port", InternalPort = 8080, ExternalPort = 8080 } })
            .SetEnv(new Dictionary<string, string?> { { "key", "envvalue" } })
            .SetSecrets(new Dictionary<string, string?> { { "key", "secretvalue" } });

        // Act
        var result = data.ToKubernetesObjects();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainItemsAssignableTo<V1Deployment>();
        result.Should().ContainItemsAssignableTo<V1Service>();
        result.Should().ContainItemsAssignableTo<V1ConfigMap>();
        result.Should().ContainItemsAssignableTo<V1Secret>();

        var deployment = result.OfType<V1Deployment>().First();
        deployment.Spec.Template.Spec.Containers[0].Name.Should().Be("test");
        deployment.Spec.Template.Spec.Containers[0].Image.Should().Be("test-image");

        var service = result.OfType<V1Service>().First();
        service.Spec.Ports[0].Name.Should().Be("test-port");
        service.Spec.Ports[0].Port.Should().Be(8080);

        var configMap = result.OfType<V1ConfigMap>().First();
        configMap.Data.Should().ContainKey("key");
        configMap.Data["key"].Should().Be("envvalue");

        var secret = result.OfType<V1Secret>().First();
        secret.Data.Should().ContainKey("key");
        secret.Data["key"].Should().NotBeEmpty().And.BeEquivalentTo("c2VjcmV0dmFsdWU="u8.ToArray());
    }

    [Fact]
    public void ToKubernetesStatefulSet_ShouldReturnCorrectStatefulSet()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image")
            .SetVolumes(new List<Volume> { new Volume { Name = "test-volume", Target = "/data", ReadOnly = false } });

        // Act
        var result = data.ToKubernetesStatefulSet();

        // Assert
        result.Spec.Template.Spec.Containers[0].Name.Should().Be("test");
        result.Spec.Template.Spec.Containers[0].Image.Should().Be("test-image");
        result.Spec.Template.Spec.Containers[0].VolumeMounts[0].Name.Should().Be("test-volume");
        result.Spec.Template.Spec.Containers[0].VolumeMounts[0].MountPath.Should().Be("/data");
        result.Spec.VolumeClaimTemplates[0].Metadata.Name.Should().Be("test-volume");
    }

    [Fact]
    public void ToKubernetesDeployment_WithBindMounts_ShouldReturnCorrectVolumes()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image")
            .SetBindMounts(new List<BindMount> { new BindMount { Name = "host", Source = "/host", Target = "/data", ReadOnly = false } });

        // Act
        var result = data.ToKubernetesDeployment();

        // Assert
        result.Spec.Template.Spec.Volumes.Should().ContainSingle(v => v.Name == "host" && v.HostPath.Path == "/host");
        result.Spec.Template.Spec.Containers[0].VolumeMounts.Should().ContainSingle(v => v.Name == "host" && v.MountPath == "/data");
    }

    [Fact]
    public void ToKubernetesDeployment_BindMountWithoutName_ShouldGenerateName()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image")
            .SetBindMounts(new List<BindMount> { new BindMount { Source = "/logs", Target = "/data", ReadOnly = false } });

        // Act
        var result = data.ToKubernetesDeployment();

        // Assert
        result.Spec.Template.Spec.Volumes.Should().ContainSingle(v => v.Name == "logs" && v.HostPath.Path == "/logs");
        result.Spec.Template.Spec.Containers[0].VolumeMounts.Should().ContainSingle(v => v.Name == "logs" && v.MountPath == "/data");
    }

    [Fact]
    public void ToKubernetesStatefulSet_WithReadOnlyVolume_ShouldContainReadOnlyFlag()
    {
        // Arrange
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("test-image")
            .SetVolumes(new List<Volume> { new Volume { Name = "test-volume", Target = "/data", ReadOnly = true } });

        // Act
        var result = data.ToKubernetesStatefulSet();
        var yaml = KubernetesYaml.Serialize(result);

        // Assert
        result.Spec.Template.Spec.Containers[0].VolumeMounts[0].ReadOnlyProperty.Should().BeTrue();
        yaml.Should().Contain("readOnly: true");
    }

    [Fact]
    public void ToKubernetesStatefulSet_MissingVolumeName_DoesNotThrow()
    {
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("img")
            .SetVolumes(new List<Volume> { new Volume { Target = "/data", ReadOnly = false } });

        var result = data.ToKubernetesStatefulSet();

        result.Spec.VolumeClaimTemplates[0].Metadata.Name.Should().BeNull();
    }

    [Fact]
    public void ToKubernetesDeployment_MissingBindMountTarget_Throws()
    {
        var data = new KubernetesDeploymentData()
            .SetName("test")
            .SetContainerImage("img")
            .SetBindMounts(new List<BindMount> { new BindMount { Name = "host", Source = "/host", ReadOnly = false } });

        Action act = () => data.ToKubernetesDeployment();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required property 'target'");
    }

    [Fact]
    public void ToKubernetesDeployment_AppliesSecurityContext()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("img")
            .SetSecurityContext(new V1PodSecurityContext { RunAsUser = 1000 }, new V1SecurityContext { RunAsUser = 1000 });

        var deployment = data.ToKubernetesDeployment();

        deployment.Spec.Template.Spec.SecurityContext.RunAsUser.Should().Be(1000);
        deployment.Spec.Template.Spec.Containers[0].SecurityContext.RunAsUser.Should().Be(1000);
    }

    [Fact]
    public void ToKubernetesStatefulSet_AppliesSecurityContext()
    {
        var data = new KubernetesDeploymentData()
            .SetName("web")
            .SetContainerImage("img")
            .SetVolumes(new List<Volume> { new Volume { Name = "data", Target = "/d", ReadOnly = false } })
            .SetSecurityContext(new V1PodSecurityContext { RunAsGroup = 2000 }, new V1SecurityContext { RunAsGroup = 2000 });

        var ss = data.ToKubernetesStatefulSet();

        ss.Spec.Template.Spec.SecurityContext.RunAsGroup.Should().Be(2000);
        ss.Spec.Template.Spec.Containers[0].SecurityContext.RunAsGroup.Should().Be(2000);
    }
}

