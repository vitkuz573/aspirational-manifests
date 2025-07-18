namespace Aspirate.Shared.Models.AspireManifests.Components.V0;

/// <summary>
/// A Dockerfile within an aspire manifest.
/// </summary>
[ExcludeFromCodeCoverage]
public class DockerfileResource : Resource, IResourceWithBinding, IResourceWithEnvironmentalVariables
{
    /// <summary>
    /// The path to the dockerfile.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// The context to pass to docker build.
    /// </summary>
    [JsonPropertyName("context")]
    public string? Context { get; set; }

    /// <summary>
    /// The bindings for the project.
    /// </summary>
    [JsonPropertyName("bindings")]
    public Dictionary<string, Binding>? Bindings { get; set; }

    [JsonPropertyName(TransformationLiterals.Env)]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("buildArgs")]
    public Dictionary<string, string>? BuildArgs { get; set; }
}
