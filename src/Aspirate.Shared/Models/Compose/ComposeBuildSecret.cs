using YamlDotNet.Serialization;

namespace Aspirate.Shared.Models.Compose;

public class ComposeBuildSecret
{
    [YamlMember(Alias = AspirateSecretLiterals.EnvironmentSecretsManagerLong)]
    public string? Environment { get; set; }

    [YamlMember(Alias = AspirateSecretLiterals.FileSecretsManager)]
    public string? File { get; set; }
}
