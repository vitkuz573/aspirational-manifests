using DockerComposeBuilder.Model.Services;
using YamlDotNet.Serialization;

namespace Aspirate.Shared.Models.Compose;

public class ComposeBuild : ServiceBuild
{
    [YamlMember(Alias = "secrets")]
    public Dictionary<string, ComposeBuildSecret>? Secrets { get; set; }
}
