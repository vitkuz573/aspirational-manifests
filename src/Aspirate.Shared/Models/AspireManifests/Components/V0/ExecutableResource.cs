namespace Aspirate.Shared.Models.AspireManifests.Components.V0;

[ExcludeFromCodeCoverage]
public class ExecutableResource : Resource,
    IResourceWithEnvironmentalVariables,
    IResourceWithBinding,
    IResourceWithArgs
{
    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("workingDirectory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName(TransformationLiterals.Env)]
    public Dictionary<string, string>? Env { get; set; } = [];

    [JsonPropertyName("bindings")]
    public Dictionary<string, Binding>? Bindings { get; set; } = [];

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; } = [];
}
