using System.Text.Json.Serialization;
using Aspirate.Shared.Models.AspireManifests.Components.Azure;

namespace Aspirate.Shared.Models.AspireManifests.Components.V1.Project;

public class ProjectV1Resource : ProjectResource
{
    [JsonPropertyName("deployment")]
    [JsonConverter(typeof(BicepResourceConverter))]
    public BicepResource? Deployment { get; set; }
}
