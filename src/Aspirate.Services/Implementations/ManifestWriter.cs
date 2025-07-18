namespace Aspirate.Services.Implementations;

public class ManifestWriter(IFileSystem fileSystem) : IManifestWriter
{
    /// <summary>
    /// Mapping of template literals to corresponding template file names.
    /// </summary>
    private readonly Dictionary<string, string> _templateFileMapping = new()
    {
        [TemplateLiterals.DeploymentType] = $"{TemplateLiterals.DeploymentType}.hbs",
        [TemplateLiterals.StatefulSetType] = $"{TemplateLiterals.StatefulSetType}.hbs",
        [TemplateLiterals.DaprComponentType] = $"{TemplateLiterals.DaprComponentType}.hbs",
        [TemplateLiterals.ServiceType] = $"{TemplateLiterals.ServiceType}.hbs",
        [TemplateLiterals.ComponentKustomizeType] = $"{TemplateLiterals.ComponentKustomizeType}.hbs",
        [TemplateLiterals.NamespaceType] = $"{TemplateLiterals.NamespaceType}.hbs",
        [TemplateLiterals.DashboardType] = $"{TemplateLiterals.DashboardType}.hbs",
        [TemplateLiterals.CloudFormationStackType] = $"{TemplateLiterals.CloudFormationStackType}.hbs",
        [TemplateLiterals.CloudFormationTemplateType] = $"{TemplateLiterals.CloudFormationTemplateType}.hbs",
    };

    /// <summary>
    /// The default path to the template folder.
    /// </summary>
    private readonly string _defaultTemplatePath = fileSystem.Path.Combine(AppContext.BaseDirectory, TemplateLiterals.TemplatesFolder);

    /// <inheritdoc />
    public void EnsureOutputDirectoryExistsAndIsClean(string outputPath)
    {
        if (fileSystem.Directory.Exists(outputPath))
        {
            fileSystem.Directory.Delete(outputPath, true);
        }

        fileSystem.Directory.CreateDirectory(outputPath);
    }

    /// <inheritdoc />
    public void CreateDeployment<TTemplateData>(string outputPath, TTemplateData data, string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.DeploymentType, out var templateFile);
        var deploymentOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.DeploymentType}.yaml");

        CreateFile(templateFile, deploymentOutputPath, data, templatePath);
    }

    public void CreateStatefulSet<TTemplateData>(string outputPath, TTemplateData data, string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.StatefulSetType, out var templateFile);
        var deploymentOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.StatefulSetType}.yaml");

        CreateFile(templateFile, deploymentOutputPath, data, templatePath);
    }

    public void CreateDaprManifest<TTemplateData>(string outputPath, TTemplateData data, string name, string? templatePath)
    {
        var daprOutputPath = fileSystem.Path.Combine(outputPath, "dapr");

        if (!fileSystem.Directory.Exists(daprOutputPath))
        {
            fileSystem.Directory.CreateDirectory(daprOutputPath);
        }

        _templateFileMapping.TryGetValue(TemplateLiterals.DaprComponentType, out var templateFile);
        var daprFileOutputPath = fileSystem.Path.Combine(daprOutputPath, $"{name}.yaml");

        CreateFile(templateFile, daprFileOutputPath, data, templatePath);
    }

    /// <inheritdoc />
    public void CreateService<TTemplateData>(string outputPath, TTemplateData data, string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.ServiceType, out var templateFile);
        var serviceOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.ServiceType}.yaml");

        CreateFile(templateFile, serviceOutputPath, data, templatePath);
    }

    /// <inheritdoc />
    public void CreateIngress<TTemplateData>(string outputPath, TTemplateData data)
    {
        if (data is not KubernetesDeploymentData deploymentData)
        {
            throw new InvalidOperationException("Ingress generation requires KubernetesDeploymentData");
        }

        var ingress = deploymentData.ToKubernetesIngress();
        var yaml = KubernetesYaml.Serialize(ingress);

        fileSystem.File.WriteAllText(fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.IngressType}.yaml"), yaml);
    }

    /// <inheritdoc />
    public void CreateComponentKustomizeManifest<TTemplateData>(
        string outputPath,
        TTemplateData data,
        string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.ComponentKustomizeType, out var templateFile);
        var kustomizeOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.ComponentKustomizeType}.yaml");

        CreateFile(templateFile, kustomizeOutputPath, data, templatePath);
    }

    /// <inheritdoc />
    public void CreateNamespace<TTemplateData>(
        string outputPath,
        TTemplateData data,
        string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.NamespaceType, out var templateFile);
        var namespaceOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.NamespaceType}.yaml");

        CreateFile(templateFile, namespaceOutputPath, data, templatePath);
    }

    public void CreateDashboard<TTemplateData>(
        string outputPath,
        TTemplateData data,
        string? templatePath)
    {
        _templateFileMapping.TryGetValue(TemplateLiterals.DashboardType, out var templateFile);
        var dashboardOutputPath = fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.DashboardType}.yaml");

        CreateFile(templateFile, dashboardOutputPath, data, templatePath);
    }

    /// <inheritdoc />
    public void CreateCustomManifest<TTemplateData>(
        string outputPath,
        string fileName,
        string templateType,
        TTemplateData data,
        string? templatePath)
    {
        _templateFileMapping.TryGetValue(templateType, out var templateFile);
        var deploymentOutputPath = fileSystem.Path.Combine(outputPath, fileName);

        CreateFile(templateFile, deploymentOutputPath, data, templatePath);
    }

    /// <inheritdoc />
    public void CreateImagePullSecret(string registryUrl, string registryUsername, string registryPassword, string registryEmail, string secretName, string outputPath)
    {
        var secretYaml = CreateImagePullSecretYaml(registryUrl, registryUsername, registryPassword, registryEmail, secretName);

        fileSystem.File.WriteAllText(fileSystem.Path.Combine(outputPath, $"{TemplateLiterals.ImagePullSecretType}.yaml"), secretYaml);
    }

    /// <inheritdoc />
    public string CreateImagePullSecretYaml(string registryUrl, string registryUsername, string registryPassword, string registryEmail, string secretName)
    {
        var dockerConfigJson = CreateDockerConfigJson(registryUrl, registryUsername, registryPassword, registryEmail);

        var secret = ImagePullSecret.Create()
            .WithName(secretName)
            .WithDockerConfigJson(dockerConfigJson);

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(secret);
    }

    private void CreateFile<TTemplateData>(string inputFile, string outputPath, TTemplateData data, string? templatePath)
    {
        var templateFile = GetTemplateFilePath(inputFile, templatePath);

        var template = fileSystem.File.ReadAllText(templateFile);
        var handlebarTemplate = Handlebars.Compile(template);
        var output = handlebarTemplate(data);

        fileSystem.File.WriteAllText(outputPath, output);
    }

    private string GetTemplateFilePath(string templateFile, string? templatePath) =>
        fileSystem.Path.Combine(templatePath ?? _defaultTemplatePath, templateFile);

    private static DockerConfigJson CreateDockerConfigJson(string registryUrl, string registryUsername, string registryPassword, string registryEmail)
    {
        string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{registryUsername}:{registryPassword}"));

        var dockerConfigJson = new DockerConfigJson
        {
            Auths = new()
            {
                [registryUrl] = new()
                {
                    Auth = auth,
                    Email = registryEmail,
                },
            },
        };

        return dockerConfigJson;
    }
}
