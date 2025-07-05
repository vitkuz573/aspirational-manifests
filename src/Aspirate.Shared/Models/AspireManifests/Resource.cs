namespace Aspirate.Shared.Models.AspireManifests;

/// <summary>
/// A resource in a manifest.
/// </summary>
[ExcludeFromCodeCoverage]
[JsonPolymorphic]
[JsonDerivedType(typeof(ProjectResource), typeDiscriminator: "aspire.project")]
[JsonDerivedType(typeof(ProjectV1Resource), typeDiscriminator: "aspire.project.v1")]
[JsonDerivedType(typeof(DockerfileResource), typeDiscriminator: "aspire.dockerfile")]
[JsonDerivedType(typeof(ContainerResource), typeDiscriminator: "aspire.container")]
[JsonDerivedType(typeof(ContainerV1Resource), typeDiscriminator: "aspire.container.v1")]
[JsonDerivedType(typeof(DaprResource), typeDiscriminator: "aspire.dapr")]
[JsonDerivedType(typeof(DaprComponentResource), typeDiscriminator: "aspire.daprcomponent")]
[JsonDerivedType(typeof(ParameterResource), typeDiscriminator: "aspire.parameter")]
[JsonDerivedType(typeof(ValueResource), typeDiscriminator: "aspire.value")]
[JsonDerivedType(typeof(ExecutableResource), typeDiscriminator: "aspire.executable")]
[JsonDerivedType(typeof(BicepResource), typeDiscriminator: "azure.bicep.v0")]
[JsonDerivedType(typeof(BicepV1Resource), typeDiscriminator: "azure.bicep.v1")]
[JsonDerivedType(typeof(CloudFormationStackResource), typeDiscriminator: "aws.cloudformation.stack.v0")]
[JsonDerivedType(typeof(CloudFormationTemplateResource), typeDiscriminator: "aws.cloudformation.template.v0")]
public abstract class Resource : IResource
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The type of the resource.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
}
