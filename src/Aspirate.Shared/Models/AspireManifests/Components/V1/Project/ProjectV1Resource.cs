using System.Text.Json.Serialization;
using Aspirate.Shared.Models.AspireManifests.Components.Azure;

namespace Aspirate.Shared.Models.AspireManifests.Components.V1.Project;

public class ProjectV1Resource : ProjectResource
{
    [JsonPropertyName("deployment")]
    public BicepV1Resource? Deployment { get; set; }
}
