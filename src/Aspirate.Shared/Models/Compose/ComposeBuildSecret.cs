using YamlDotNet.Serialization;

namespace Aspirate.Shared.Models.Compose;

public class ComposeBuildSecret
{
    [YamlMember(Alias = "environment")]
    public string? Environment { get; set; }

    [YamlMember(Alias = "file")]
    public string? File { get; set; }
}
