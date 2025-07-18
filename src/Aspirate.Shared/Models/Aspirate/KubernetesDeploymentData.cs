using Volume = Aspirate.Shared.Models.AspireManifests.Components.Common.Volume;
using BindMount = Aspirate.Shared.Models.AspireManifests.Components.Common.BindMount;
using k8s.Models;

namespace Aspirate.Shared.Models.Aspirate;

[ExcludeFromCodeCoverage]
public class KubernetesDeploymentData
{
    public string? Name {get; private set;}
    public string? Namespace {get; private set;}
    public Dictionary<string, string?> Env { get; private set; } = [];
    public Dictionary<string, string?> Secrets { get; private set; } = [];
    public Dictionary<string, string> Annotations { get; private set; } = [];
    public Dictionary<string, string> IngressAnnotations { get; private set; } = [];
    public IReadOnlyCollection<Volume> Volumes { get; private set; } = [];
    public IReadOnlyCollection<BindMount> BindMounts { get; private set; } = [];
    public IReadOnlyCollection<Ports> Ports { get; private set; } = [];
    public IReadOnlyCollection<string> Manifests { get; private set; } = [];
    public IReadOnlyCollection<string> Args { get; private set; } = [];
    public bool? SecretsDisabled { get; private set; } = false;
    public bool? IsProject {get; private set;}
    public bool? WithPrivateRegistry { get; private set; } = false;
    public bool? WithDashboard { get; private set; } = false;
    public string? ContainerImage {get; private set;}
    public string? Entrypoint {get; private set;}
    public string? ImagePullPolicy {get; private set;}
    public string? ServiceType { get; private set; } = "ClusterIP";
    public bool? IngressEnabled { get; private set; } = false;
    public string? IngressClassName { get; private set; } = IngressController.Nginx.Value;
    public IReadOnlyCollection<string> IngressHosts { get; private set; } = [];
    public string? IngressTlsSecret { get; private set; }
    public string? IngressPath { get; private set; }
    public int? IngressPortNumber { get; private set; }
    public BicepResource? Deployment { get; private set; }
    public V1PodSecurityContext? PodSecurityContext { get; private set; }
    public V1SecurityContext? ContainerSecurityContext { get; private set; }

    public KubernetesDeploymentData SetName(string name)
    {
        Name = name.ToLowerInvariant();
        return this;
    }

    public KubernetesDeploymentData SetNamespace(string? ns)
    {
        if (string.IsNullOrEmpty(ns))
        {
            return this;
        }

        Namespace = ns.ToLowerInvariant();
        return this;
    }

    public KubernetesDeploymentData SetEnv(Dictionary<string, string?> env)
    {
        Env = env.Where(x=>!string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        return this;
    }

    public KubernetesDeploymentData SetSecrets(Dictionary<string, string?> secrets)
    {
        Secrets = secrets;
        return this;
    }

    public KubernetesDeploymentData SetAnnotations(Dictionary<string, string>? annotations)
    {
        Annotations = annotations ?? [];
        return this;
    }

    public KubernetesDeploymentData SetIngressAnnotations(Dictionary<string, string>? annotations)
    {
        IngressAnnotations = annotations ?? [];
        return this;
    }

    public KubernetesDeploymentData SetManifests(IReadOnlyCollection<string> manifests)
    {
        Manifests = manifests;
        return this;
    }

    public KubernetesDeploymentData SetArgs(IReadOnlyCollection<string>? args)
    {
        Args = args ?? [];
        return this;
    }

    public KubernetesDeploymentData SetIsProject(bool project)
    {
        IsProject = project;
        return this;
    }

    public KubernetesDeploymentData SetEntrypoint(string? entrypoint)
    {
        Entrypoint = entrypoint;
        return this;
    }

    public KubernetesDeploymentData SetVolumes(List<Volume>? volumes)
    {
        Volumes = volumes ?? [];
        return this;
    }

    public KubernetesDeploymentData SetBindMounts(List<BindMount>? bindMounts)
    {
        BindMounts = bindMounts ?? [];
        return this;
    }

    public KubernetesDeploymentData SetWithPrivateRegistry(bool isPrivateRegistry)
    {
        WithPrivateRegistry = isPrivateRegistry;
        return this;
    }

    public KubernetesDeploymentData SetContainerImage(string containerImage)
    {
        ContainerImage = containerImage.ToLowerInvariant();
        return this;
    }

    public KubernetesDeploymentData SetImagePullPolicy(string? imagePullPolicy)
    {
        ImagePullPolicy = imagePullPolicy ?? "IfNotPresent";
        return this;
    }

    public KubernetesDeploymentData SetIngressClassName(string? className)
    {
        IngressClassName = className ?? IngressController.Nginx.Value;
        return this;
    }

    public KubernetesDeploymentData SetWithDashboard(bool? withDashboard)
    {
        WithDashboard = withDashboard ?? false;
        return this;
    }

    public KubernetesDeploymentData SetPorts(List<Ports>? ports)
    {
        Ports = ports ?? [];
        return this;
    }

    public KubernetesDeploymentData SetServiceType(string? serviceType)
    {
        ServiceType = serviceType ?? "ClusterIP";
        return this;
    }

    public KubernetesDeploymentData SetIngressEnabled(bool enabled)
    {
        IngressEnabled = enabled;
        return this;
    }

    public KubernetesDeploymentData SetIngressHosts(IReadOnlyCollection<string> hosts)
    {
        IngressHosts = hosts;
        return this;
    }

    public KubernetesDeploymentData SetIngressTlsSecret(string? secret)
    {
        IngressTlsSecret = secret;
        return this;
    }

    public KubernetesDeploymentData SetIngressPath(string? path)
    {
        IngressPath = path;
        return this;
    }

    public KubernetesDeploymentData SetIngressPortNumber(int? port)
    {
        IngressPortNumber = port;
        return this;
    }

    public KubernetesDeploymentData SetDeployment(BicepResource? deployment)
    {
        Deployment = deployment;

        if (deployment is BicepV1Resource v1 && v1.Scope?.ResourceGroup is { } rg)
        {
            Annotations ??= [];

            if (!Annotations.ContainsKey("aspirate.io/bicep-resource-group"))
            {
                Annotations.Add("aspirate.io/bicep-resource-group", rg);
            }
            else
            {
                Annotations["aspirate.io/bicep-resource-group"] = rg;
            }
        }

        return this;
    }

    public KubernetesDeploymentData SetSecurityContext(V1PodSecurityContext? podContext, V1SecurityContext? containerContext = null)
    {
        PodSecurityContext = podContext;
        ContainerSecurityContext = containerContext;
        return this;
    }

    public KubernetesDeploymentData SetSecretsFromSecretState(KeyValuePair<string, Resource> resource, ISecretProvider secretProvider, bool? disableSecrets = false)
    {
        if (disableSecrets == true)
        {
            SecretsDisabled = true;
            return this;
        }

        if (!secretProvider.ResourceExists(resource.Key))
        {
            return this;
        }

        for (int i = 0; i < Secrets.Count; i++)
        {
            var secret = Secrets.ElementAt(i);

            if (!secretProvider.SecretExists(resource.Key, secret.Key))
            {
                continue;
            }

            var encryptedSecret = secretProvider.GetSecret(resource.Key, secret.Key);

            Secrets[secret.Key] = encryptedSecret;
        }

        return this;
    }

    public bool HasPorts => Ports.Count > 0;
    public bool HasVolumes => Volumes.Count > 0;
    public bool HasBindMounts => BindMounts.Count > 0;
    public bool HasAnySecrets => Secrets.Count > 0 && SecretsDisabled != true;
    public bool HasAnyAnnotations => Annotations.Count > 0;
    public bool HasIngressAnnotations => IngressAnnotations.Count > 0;
    public bool HasArgs => Args.Count > 0;
    public bool HasAnyEnv => Env.Count > 0;
    public bool WithNamespace => !string.IsNullOrWhiteSpace(Namespace);

    public KubernetesDeploymentData Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Name must be set.");
        }

        return this;
    }
}

