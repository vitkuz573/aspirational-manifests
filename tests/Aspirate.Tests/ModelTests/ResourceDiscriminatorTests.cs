using System.Text.Json;
using Xunit;

namespace Aspirate.Tests.ModelTests;

public class ResourceDiscriminatorTests
{
    public static IEnumerable<object[]> ResourceData => new List<object[]>
    {
        new object[] { new ProjectResource(), AspireComponentLiterals.Project, typeof(ProjectResource) },
        new object[] { new ProjectV1Resource(), AspireComponentLiterals.ProjectV1, typeof(ProjectV1Resource) },
        new object[] { new DockerfileResource(), AspireComponentLiterals.Dockerfile, typeof(DockerfileResource) },
        new object[] { new ContainerResource { Image = "img" }, AspireComponentLiterals.Container, typeof(ContainerResource) },
        new object[] { new ContainerV1Resource { Image = "img" }, AspireComponentLiterals.ContainerV1, typeof(ContainerV1Resource) },
        new object[] { new DaprResource(), AspireComponentLiterals.DaprSystem, typeof(DaprResource) },
        new object[] { new DaprComponentResource(), AspireComponentLiterals.DaprComponent, typeof(DaprComponentResource) },
        new object[] { new ParameterResource(), AspireComponentLiterals.Parameter, typeof(ParameterResource) },
        new object[] { new ValueResource { ConnectionString = "c" }, AspireComponentLiterals.Value, typeof(ValueResource) },
        new object[] { new ExecutableResource(), AspireComponentLiterals.Executable, typeof(ExecutableResource) },
        new object[] { new BicepResource(), AspireComponentLiterals.AzureBicep, typeof(BicepResource) },
        new object[] { new BicepV1Resource(), AspireComponentLiterals.AzureBicepV1, typeof(BicepV1Resource) },
        new object[] { new CloudFormationStackResource(), AspireComponentLiterals.AwsCloudFormationStack, typeof(CloudFormationStackResource) },
        new object[] { new CloudFormationTemplateResource(), AspireComponentLiterals.AwsCloudFormationTemplate, typeof(CloudFormationTemplateResource) },
    };

    [Theory]
    [MemberData(nameof(ResourceData))]
    public void Resource_Serializes_WithSchemaType(Resource resource, string expectedType, Type expected)
    {
        resource.Type = expectedType;
        var json = JsonSerializer.Serialize<Resource>(resource);
        json.Should().Contain($"\"type\":\"{expectedType}\"");

        var roundTrip = JsonSerializer.Deserialize<Resource>(json)!;
        roundTrip.Should().BeOfType(expected);
    }
}
