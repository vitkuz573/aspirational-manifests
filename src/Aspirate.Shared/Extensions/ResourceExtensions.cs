using Aspirate.Shared.Models.AspireManifests.Components.V1.Project;
using Volume = Aspirate.Shared.Models.AspireManifests.Components.Common.Volume;
using BindMount = Aspirate.Shared.Models.AspireManifests.Components.Common.BindMount;
using Aspirate.Shared.Literals;

namespace Aspirate.Shared.Extensions;

public static class ResourceExtensions
{
    public static Dictionary<string, string?> MapResourceToEnvVars(this KeyValuePair<string, Resource> resource, bool? withDashboard)
    {
        var environment = new Dictionary<string, string?>();

        if (resource.Value is ProjectV1Resource projectV1)
        {
            var envVars = projectV1.Env;

            if (envVars != null)
            {
                foreach (var entry in envVars.Where(entry => !string.IsNullOrEmpty(entry.Value)))
                {
                    environment.Add(entry.Key, entry.Value);
                }
            }
        }
        else if (resource.Value is IResourceWithEnvironmentalVariables { Env: not null } resourceWithEnv)
        {
            foreach (var entry in resourceWithEnv.Env.Where(entry => !string.IsNullOrEmpty(entry.Value)))
            {
                environment.Add(entry.Key, entry.Value);
            }
        }
        else
        {
            return environment;
        }

        if (withDashboard == true)
        {
            environment.TryAdd("OTEL_EXPORTER_OTLP_ENDPOINT", "http://aspire-dashboard:18889");
            environment.TryAdd("OTEL_SERVICE_NAME", resource.Key);
        }

        return environment;
    }

    public static List<Volume> KuberizeVolumeNames(this List<Volume> containerVolumes,  KeyValuePair<string, Resource> resource)
    {
        if (containerVolumes.Count == 0)
        {
            return containerVolumes;
        }

        foreach (var volume in containerVolumes)
        {
            if (string.IsNullOrWhiteSpace(volume.Target))
            {
                throw new InvalidOperationException($"Volume missing required property 'target'.");
            }

            if (string.IsNullOrWhiteSpace(volume.Name))
            {
                throw new InvalidOperationException($"Volume missing required property 'name'.");
            }

            if (volume.ReadOnly is null)
            {
                throw new InvalidOperationException($"Volume missing required property 'readOnly'.");
            }

            volume.Name = volume.Name.Replace("/", "-").Replace(".", "-").Replace("--", "-").ToLowerInvariant();
        }

        return containerVolumes;
    }

    public static List<BindMount> KuberizeBindMountNames(this List<BindMount> bindMounts,  KeyValuePair<string, Resource> resource)
    {
        if (bindMounts.Count == 0)
        {
            return bindMounts;
        }

        var usedNames = new HashSet<string>();

        foreach (var mount in bindMounts)
        {
            if (string.IsNullOrWhiteSpace(mount.Source))
            {
                throw new InvalidOperationException($"BindMount missing required property 'source'.");
            }

            if (string.IsNullOrWhiteSpace(mount.Target))
            {
                throw new InvalidOperationException($"BindMount missing required property 'target'.");
            }

            if (mount.ReadOnly is null)
            {
                throw new InvalidOperationException($"BindMount missing required property 'readOnly'.");
            }

            if (string.IsNullOrWhiteSpace(mount.Name))
            {
                var generated = mount.Source
                    .Replace("/", "-")
                    .Replace(".", "-")
                    .Trim('-')
                    .ToLowerInvariant();

                var baseName = generated;
                var index = 1;
                while (!usedNames.Add(generated))
                {
                    generated = $"{baseName}-{index++}";
                }

                mount.Name = generated;
            }

            mount.Name = mount.Name.Replace("/", "-").Replace(".", "-").Replace("--", "-").ToLowerInvariant();
            usedNames.Add(mount.Name);
        }

        return bindMounts;
    }

    public static string[] MapComposeVolumes(this KeyValuePair<string, Resource> resource)
    {
        var composeVolumes = new List<string>();

        if (resource.Value is IResourceWithVolumes resourceWithVolumes)
        {
            KuberizeVolumeNames(resourceWithVolumes.Volumes, resource);
            composeVolumes.AddRange(resourceWithVolumes.Volumes
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(volume => $"{volume.Name}:{volume.Target}"));
        }

        if (resource.Value is IResourceWithBindMounts resourceWithBindMounts)
        {
            KuberizeBindMountNames(resourceWithBindMounts.BindMounts, resource);
            composeVolumes.AddRange(resourceWithBindMounts.BindMounts?
                .Where(x => !string.IsNullOrWhiteSpace(x.Source) && !string.IsNullOrWhiteSpace(x.Target))
                .Select(m => $"{m.Source}:{m.Target}{(m.ReadOnly == true ? ":ro" : string.Empty)}") ?? []);
        }

        return composeVolumes.ToArray();
    }

    public static List<Ports> MapBindingsToPorts(this KeyValuePair<string, Resource> resource)
    {
        Dictionary<string, Binding>? bindings = null;

        if (resource.Value is ProjectV1Resource projectV1)
        {
            bindings = projectV1.Bindings;
        }
        else if (resource.Value is IResourceWithBinding resourceWithBinding)
        {
            bindings = resourceWithBinding.Bindings;
        }

        return bindings?.Select(b => new Ports
            {
                Name = b.Key,
                InternalPort = b.Value.TargetPort.GetValueOrDefault(),
                ExternalPort = b.Value.Port.GetValueOrDefault()
            }).ToList() ?? [];
    }

    public static Port[] MapPortsToDockerComposePorts(this List<Ports> ports) =>
        ports.Select(x=> new Port
        {
            Target = x.InternalPort,
            Published = x.ExternalPort != 0 ? x.ExternalPort : x.InternalPort,
        }).ToArray();

    public static void EnsureBindingsHavePorts(this Dictionary<string, Resource> resources)
    {
        foreach (var resource in resources.Where(x=>x.Value is IResourceWithBinding {Bindings: not null}))
        {
            var bindingResource = resource.Value as IResourceWithBinding;

            foreach (var binding in bindingResource.Bindings)
            {
                if (binding.Key.Equals(BindingLiterals.Http, StringComparison.OrdinalIgnoreCase) && binding.Value.TargetPort is 0 or null)
                {
                    binding.Value.TargetPort = 8080;
                }

                if (binding.Key.Equals(BindingLiterals.Https, StringComparison.OrdinalIgnoreCase) && binding.Value.TargetPort is 0 or null)
                {
                    binding.Value.TargetPort = 8443;
                }
            }
        }
    }
}
