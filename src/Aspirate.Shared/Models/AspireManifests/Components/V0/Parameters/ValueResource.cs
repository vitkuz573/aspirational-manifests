namespace Aspirate.Shared.Models.AspireManifests.Components.V0.Parameters;

public class ValueResource : Resource
{
    [JsonPropertyName("connectionString")]
    public required string ConnectionString { get; set; }
}
