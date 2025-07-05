using System.Text.Json.Serialization;
namespace Aspirate.Shared.Models.AspireManifests.Components.V1.Project;

public class ProjectV1Resource : ProjectResource
{
    private Deployment? _deployment;

    [JsonPropertyName("deployment")]
    public Deployment? Deployment
    {
        get => _deployment;
        set
        {
            _deployment = value;
            if (value != null)
            {
                Bindings = value.Bindings ?? Bindings;
                Annotations = value.Annotations ?? Annotations;
                Env = value.Env ?? Env;
                Args = value.Args ?? Args;
            }
        }
    }
}
