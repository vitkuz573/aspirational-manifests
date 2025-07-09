using Aspirate.Shared.Models.AspireManifests.Components;
using System.Text.Json.Nodes;

namespace Aspirate.Services.Implementations;

/// <inheritdoc />
/// <summary>
/// Initialises a new instance of <see cref="ManifestFileParserService"/>.
/// </summary>
/// <param name="fileSystem">The file system accessor.</param>
/// <param name="console">The ansi-console instance used for console interaction.</param>
/// <param name="serviceProvider">The service provider to resolve handlers from.</param>
public class ManifestFileParserService(
    IFileSystem fileSystem,
    IAnsiConsole console,
    IServiceProvider serviceProvider) : IManifestFileParserService
{
    public string? ManifestDirectory { get; private set; }

    private static readonly Dictionary<string, HashSet<string>> _allowedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        [AspireComponentLiterals.Dockerfile] = new(
        [
            "type",
            "path",
            "context",
            TransformationLiterals.Env,
            "bindings",
            "buildArgs"
        ]),
        [AspireComponentLiterals.Container] = new(
        [
            "type",
            "image",
            "entrypoint",
            "args",
            "connectionString",
            TransformationLiterals.Env,
            "bindings",
            "bindMounts",
            "volumes"
        ]),
        [AspireComponentLiterals.ContainerV1] = new(
        [
            "type",
            "image",
            "entrypoint",
            "deployment",
            "args",
            "build",
            "connectionString",
            TransformationLiterals.Env,
            "bindings",
            "bindMounts",
            "volumes"
        ]),
        [AspireComponentLiterals.Project] = new(
        [
            "type",
            "path",
            "args",
            TransformationLiterals.Env,
            "bindings"
        ]),
        [AspireComponentLiterals.ProjectV1] = new(
        [
            "type",
            "path",
            "deployment",
            "args",
            TransformationLiterals.Env,
            "bindings"
        ]),
        [AspireComponentLiterals.Executable] = new(
        [
            "type",
            "command",
            "workingDirectory",
            "args",
            TransformationLiterals.Env,
            "bindings"
        ]),
        [AspireComponentLiterals.Value] = new(
        [
            "type",
            "connectionString"
        ]),
        [AspireComponentLiterals.Parameter] = new(
        [
            "type",
            "value",
            "connectionString",
            "inputs"
        ]),
        [AspireComponentLiterals.DaprSystem] = new(
        [
            "type",
            "dapr"
        ]),
        [AspireComponentLiterals.DaprComponent] = new(
        [
            "type",
            "daprComponent"
        ]),
        [AspireComponentLiterals.AzureBicep] = new(
        [
            "type",
            "path",
            "connectionString",
            "params"
        ]),
        [AspireComponentLiterals.AzureBicepV1] = new(
        [
            "type",
            "path",
            "connectionString",
            "params",
            "scope"
        ]),
        [AspireComponentLiterals.AwsCloudFormationStack] = new(
        [
            "type",
            "stack-name",
            "references"
        ]),
        [AspireComponentLiterals.AwsCloudFormationTemplate] = new(
        [
            "type",
            "stack-name",
            "template-path",
            "references"
        ])
    };

    private static void ValidateProperties(string resourceName, string type, JsonElement element)
    {
        if (!_allowedProperties.TryGetValue(type, out var allowed))
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw new InvalidOperationException($"{type} {resourceName} unexpected property '{property.Name}'.");
            }
        }
    }
    /// <inheritdoc />
    public Dictionary<string, Resource> LoadAndParseAspireManifest(string manifestFile)
    {
        var resources = new Dictionary<string, Resource>();

        if (!fileSystem.File.Exists(manifestFile))
        {
            throw new InvalidOperationException($"The manifest file could not be loaded from: '{manifestFile}'");
        }

        ManifestDirectory = fileSystem.Path.GetDirectoryName(fileSystem.GetFullPath(manifestFile));

        var inputJson = fileSystem.File.ReadAllText(manifestFile);

        var jsonObject = JsonSerializer.Deserialize<JsonElement>(inputJson);

        if (!jsonObject.TryGetProperty("resources", out var resourcesElement) || resourcesElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("The manifest file does not contain a 'resources' object.");
        }

        foreach (var resourceProperty in resourcesElement.EnumerateObject())
        {
            var resourceName = resourceProperty.Name;
            var resourceElement = resourceProperty.Value;

            var type = resourceElement.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;

            if (type == null)
            {
                console.MarkupLine($"[yellow]Resource '{resourceName}' does not have a type. Skipping as UnsupportedResource.[/]");
                resources.Add(resourceName, new UnsupportedResource());
                continue;
            }

            if (type == AspireComponentLiterals.Container && resourceElement.TryGetProperty("build", out _))
            {
                throw new InvalidOperationException($"{AspireComponentLiterals.Container} {resourceName} does not support property 'build'.");
            }

            ValidateProperties(resourceName, type, resourceElement);

            var rawBytes = Encoding.UTF8.GetBytes(resourceElement.GetRawText());
            var reader = new Utf8JsonReader(rawBytes);

            var resourceProcessor = serviceProvider.GetKeyedService<IResourceProcessor>(type);

            Resource resource;

            if (resourceProcessor != null)
            {
                resource = resourceProcessor.Deserialize(ref reader);
            }
            else
            {
                console.MarkupLine($"[yellow]Resource '{resourceName}' is unsupported type '{type}'. Preserving as ExtensionResource.[/]");
                var jsonNode = JsonNode.Parse(resourceElement.GetRawText());
                resource = new ExtensionResource(jsonNode!, type);
            }

            if (resource != null)
            {
                resource.Name = resourceName;
                resources.Add(resourceName, resource);
            }
        }

        return resources;
    }
}
