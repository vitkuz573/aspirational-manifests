namespace Aspirate.Shared.Extensions;

public static class KubernetesDeploymentDataExtensions
{
    public static Dictionary<string, string> ToKubernetesLabels(this KubernetesDeploymentData data) =>
        new()
        {
            ["app"] = data.Name,
        };

    public static V1ObjectMeta ToKubernetesObjectMetaData(this KubernetesDeploymentData data, Dictionary<string, string>? labels = null)
    {
        labels ??= data.ToKubernetesLabels();

        return new V1ObjectMeta
        {
            Name = $"{data.Name.ToLowerInvariant()}",
            NamespaceProperty = data.Namespace,
            Annotations = data.Annotations,
            Labels = labels,
        };
    }

    public static V1ConfigMap ToKubernetesConfigMap(this KubernetesDeploymentData data, Dictionary<string, string>? labels = null)
    {
        labels ??= data.ToKubernetesLabels();
        var metadata = data.ToKubernetesObjectMetaData(labels);

        return new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Metadata = metadata,
            Data = data.Env,
        };
    }

    public static V1Secret ToKubernetesSecret(this KubernetesDeploymentData data, bool? encodeSecrets = true, Dictionary<string, string>? labels = null)
    {
        labels ??= data.ToKubernetesLabels();
        var metadata = data.ToKubernetesObjectMetaData(labels);

        var secrets = new Dictionary<string, byte[]>();

        foreach (var secret in data.Secrets.Where(secret => !string.IsNullOrEmpty(secret.Value)))
        {
            var secretValueBytes = Encoding.UTF8.GetBytes(secret.Value);
            secrets[secret.Key] = encodeSecrets == true ? Encoding.UTF8.GetBytes(Convert.ToBase64String(secretValueBytes)) : secretValueBytes;
        }

        return new V1Secret
        {
            ApiVersion = "v1",
            Kind = "Secret",
            Metadata = metadata,
            Type = "Opaque",
            Data = secrets,
        };
    }

    public static V1Container ToKubernetesContainer(this KubernetesDeploymentData data, bool useConfigMap = true, bool useSecrets = true)
    {
        var container = new V1Container
        {
            Name = data.Name,
            Image = data.ContainerImage,
            ImagePullPolicy = data.ImagePullPolicy,
            Args = data.Args.ToArray(),
        };

        if (!string.IsNullOrEmpty(data.Entrypoint))
        {
            container.Command = new List<string> { data.Entrypoint };
        }

        if (data.HasPorts)
        {
            container.Ports = data.Ports.Select(x => new V1ContainerPort
            {
                ContainerPort = x.InternalPort,
                Name = x.Name
            }).ToList();
        }

        if (data.HasAnyEnv)
        {
            SetContainerEnvironment(data, useConfigMap, useSecrets, container);
        }


        var volumeMounts = new List<V1VolumeMount>();

        if (data.HasVolumes)
        {
            volumeMounts.AddRange(data.Volumes.Select(x =>
            {
                var mount = new V1VolumeMount
                {
                    Name = x.Name,
                    MountPath = x.Target,
                };

                if (x.ReadOnly == true)
                {
                    mount.ReadOnlyProperty = true;
                }

                return mount;
            }));
        }

        if (data.HasBindMounts)
        {
            volumeMounts.AddRange(data.BindMounts.Select(x => new V1VolumeMount
            {
                Name = x.Name,
                MountPath = x.Target,
                ReadOnlyProperty = x.ReadOnly == true,
            }));
        }

        if (volumeMounts.Count > 0)
        {
            container.VolumeMounts = volumeMounts;
        }

        return container;
    }

    public static IList<V1PersistentVolumeClaim> ToKubernetesPersistentVolumeClaimTemplates(this KubernetesDeploymentData data) =>
        data.Volumes.Select(x => new V1PersistentVolumeClaim
        {
            Metadata = new V1ObjectMeta
            {
                Name = x.Name,
            },
            Spec = new V1PersistentVolumeClaimSpec
            {
                AccessModes = new List<string> { "ReadWriteOnce" },
                Resources = new V1VolumeResourceRequirements
                {
                    Requests = new Dictionary<string, ResourceQuantity>
                    {
                        ["storage"] = new("1Gi"),
                    },
                },
            },
        }).ToList();

    public static V1Deployment ToKubernetesDeployment(this KubernetesDeploymentData data,
        Dictionary<string, string>? labels = null,
        bool useConfigMap = true,
        bool useSecrets = true,
        V1Container? container = null)
    {
        labels ??= data.ToKubernetesLabels();
        container ??= data.ToKubernetesContainer(useConfigMap, useSecrets);
        var metadata = data.ToKubernetesObjectMetaData(labels);

        var deployment = new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = metadata,
            Spec = new V1DeploymentSpec
            {
                Selector = new V1LabelSelector
                {
                    MatchLabels = labels,
                },
                Strategy = new V1DeploymentStrategy
                {
                    Type = "Recreate",
                },
                MinReadySeconds = 60,
                Replicas = 1,
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = labels,
                        Annotations = metadata.Annotations,
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container> { container },
                        TerminationGracePeriodSeconds = 180,
                        Volumes = data.HasBindMounts
                            ? data.BindMounts.Select(x => new V1Volume
                                {
                                    Name = x.Name,
                                    HostPath = new V1HostPathVolumeSource { Path = x.Source }
                                }).ToList()
                            : null,
                    },
                },
            },
        };

        if (data.WithPrivateRegistry == true)
        {
            deployment.Spec.Template.Spec.ImagePullSecrets = SetKubernetesImagePullSecrets;
        }

        return deployment;
    }

    public static V1StatefulSet ToKubernetesStatefulSet(this KubernetesDeploymentData data, Dictionary<string, string>? labels = null, bool useConfigMap = true, bool useSecrets = true)
    {
        labels ??= data.ToKubernetesLabels();
        var metadata = data.ToKubernetesObjectMetaData(labels);

        var statefulSet = new V1StatefulSet
        {
            ApiVersion = "apps/v1",
            Kind = "StatefulSet",
            Metadata = metadata,
            Spec = new V1StatefulSetSpec
            {
                Selector = new V1LabelSelector
                {
                    MatchLabels = labels,
                },
                ServiceName = data.Name,
                Replicas = 1,
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = labels,
                        Annotations = metadata.Annotations,
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container> { data.ToKubernetesContainer(useConfigMap, useSecrets) },
                        TerminationGracePeriodSeconds = 180,
                        Volumes = data.HasBindMounts
                            ? data.BindMounts.Select(x => new V1Volume
                                {
                                    Name = x.Name,
                                    HostPath = new V1HostPathVolumeSource { Path = x.Source }
                                }).ToList()
                            : null,
                    },
                },
            },
        };

        if (data.WithPrivateRegistry == true)
        {
            statefulSet.Spec.Template.Spec.ImagePullSecrets = SetKubernetesImagePullSecrets;
        }

        if (data.HasVolumes)
        {
            statefulSet.Spec.VolumeClaimTemplates = data.ToKubernetesPersistentVolumeClaimTemplates();
        }

        return statefulSet;
    }

    public static V1Service ToKubernetesService(this KubernetesDeploymentData data)
    {
        var labels = data.ToKubernetesLabels();
        var metadata = data.ToKubernetesObjectMetaData(labels);

        return new V1Service
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = metadata,
            Spec = new V1ServiceSpec
            {
                Selector = labels,
                Ports = data.Ports.Select(x => new V1ServicePort
                {
                    Port = x.ExternalPort,
                    TargetPort = x.InternalPort,
                    Name = x.Name
                }).ToList(),
                Type = data.ServiceType,
                ClusterIP = data.HasPorts ? null : "None",
            },
        };
    }

    public static V1Ingress ToKubernetesIngress(this KubernetesDeploymentData data)
    {
        var labels = data.ToKubernetesLabels();
        var metadata = data.ToKubernetesObjectMetaData(labels);

        var firstPort = data.Ports.FirstOrDefault();
        var servicePort = data.IngressPortNumber
            ?? (firstPort?.ExternalPort > 0 ? firstPort.ExternalPort : (int?)null)
            ?? firstPort?.InternalPort
            ?? 80;

        var ingress = new V1Ingress
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "Ingress",
            Metadata = metadata,
            Spec = new V1IngressSpec
            {
                IngressClassName = "nginx",
                Rules =
                [
                    new V1IngressRule
                    {
                        Host = data.IngressHost,
                        Http = new V1HTTPIngressRuleValue
                        {
                            Paths =
                            [
                                new V1HTTPIngressPath
                                {
                                    Path = data.IngressPath ?? "/",
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = data.Name,
                                            Port = new V1ServiceBackendPort { Number = servicePort }
                                        }
                                    }
                                }
                            ]
                        }
                    }
                ]
            }
        };

        if (!string.IsNullOrEmpty(data.IngressTlsSecret))
        {
            ingress.Spec.Tls =
            [
                new V1IngressTLS
                {
                    SecretName = data.IngressTlsSecret,
                    Hosts = [ data.IngressHost ]
                }
            ];
        }

        return ingress;
    }

    public static List<object> ToKubernetesObjects(this KubernetesDeploymentData data, bool? encodeSecrets = true)
    {
        var objects = new List<object>();

        if (data.HasAnyEnv)
        {
            objects.Add(data.ToKubernetesConfigMap());
        }

        if (data.HasAnySecrets)
        {
            objects.Add(data.ToKubernetesSecret(encodeSecrets));
        }

        switch (data.HasVolumes)
        {
            case true:
                objects.Add(data.ToKubernetesStatefulSet());
                break;
            case false:
                objects.Add(data.ToKubernetesDeployment());
                break;
        }

        objects.Add(data.ToKubernetesService());

        if (data.IngressEnabled == true)
        {
            objects.Add(data.ToKubernetesIngress());
        }

        return objects;
    }

    private static List<V1LocalObjectReference> SetKubernetesImagePullSecrets =>
    [
        new V1LocalObjectReference { Name = TemplateLiterals.ImagePullSecretType, }
    ];

    private static void SetContainerEnvironment(KubernetesDeploymentData data, bool useConfigMap, bool useSecrets, V1Container container)
    {
        if (!useConfigMap && !useSecrets)
        {
            container.Env = data.Env.Select(x => new V1EnvVar { Name = x.Key, Value = x.Value, }).ToList();
            return;
        }

        container.EnvFrom = new List<V1EnvFromSource>();

        if (useConfigMap && data.HasAnyEnv)
        {
            container.EnvFrom.Add(new V1EnvFromSource
            {
                ConfigMapRef = new V1ConfigMapEnvSource { Name = data.Name.ToLowerInvariant(), },
            });
        }

        if (useSecrets && data.HasAnySecrets)
        {
            container.EnvFrom.Add(new V1EnvFromSource
            {
                SecretRef = new V1SecretEnvSource { Name = data.Name.ToLowerInvariant(), },
            });
        }
    }

    public static KubernetesDeploymentData ApplyAnnotations(this KubernetesDeploymentData data, BaseKubernetesCreateOptions options)
    {
        var state = options.CurrentState;
        if (state?.ResourceAnnotations != null && state.ResourceAnnotations.TryGetValue(options.Resource.Key, out var annotations))
        {
            foreach (var annotation in annotations)
            {
                data.Annotations[annotation.Key] = annotation.Value;
            }
        }

        return data;
    }

    public static KubernetesDeploymentData ApplyIngress(this KubernetesDeploymentData data, BaseKubernetesCreateOptions options)
    {
        var state = options.CurrentState;
        if (state?.IngressDefinitions != null && state.IngressDefinitions.TryGetValue(options.Resource.Key, out var def))
        {
            data
                .SetIngressEnabled(true)
                .SetIngressHost(def.Host)
                .SetIngressTlsSecret(def.TlsSecret)
                .SetIngressPath(def.Path)
                .SetIngressPortNumber(def.PortNumber);
        }

        return data;
    }
}
