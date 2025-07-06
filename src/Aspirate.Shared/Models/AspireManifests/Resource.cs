using Aspirate.Shared.Literals;

namespace Aspirate.Shared.Models.AspireManifests;

/// <summary>
/// A resource in a manifest.
/// </summary>
[ExcludeFromCodeCoverage]
[JsonPolymorphic]
[JsonDerivedType(typeof(ProjectResource), typeDiscriminator: AspireComponentLiterals.Project)]
[JsonDerivedType(typeof(ProjectV1Resource), typeDiscriminator: AspireComponentLiterals.ProjectV1)]
[JsonDerivedType(typeof(DockerfileResource), typeDiscriminator: AspireComponentLiterals.Dockerfile)]
[JsonDerivedType(typeof(ContainerResource), typeDiscriminator: AspireComponentLiterals.Container)]
[JsonDerivedType(typeof(ContainerV1Resource), typeDiscriminator: AspireComponentLiterals.ContainerV1)]
[JsonDerivedType(typeof(DaprResource), typeDiscriminator: AspireComponentLiterals.DaprSystem)]
[JsonDerivedType(typeof(DaprComponentResource), typeDiscriminator: AspireComponentLiterals.DaprComponent)]
[JsonDerivedType(typeof(ParameterResource), typeDiscriminator: AspireComponentLiterals.Parameter)]
[JsonDerivedType(typeof(ValueResource), typeDiscriminator: AspireComponentLiterals.Value)]
[JsonDerivedType(typeof(ExecutableResource), typeDiscriminator: AspireComponentLiterals.Executable)]
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
