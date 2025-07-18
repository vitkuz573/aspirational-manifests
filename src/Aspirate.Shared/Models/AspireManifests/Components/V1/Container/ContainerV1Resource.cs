using Aspirate.Shared.Literals;

namespace Aspirate.Shared.Models.AspireManifests.Components.V1.Container;

public class ContainerV1Resource : ContainerResourceBase
{
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("build")]
    public Build? Build { get; set; }

    [JsonPropertyName(TemplateLiterals.DeploymentType)]
    [JsonConverter(typeof(BicepResourceConverter))]
    public BicepResource? Deployment { get; set; }
}
