using YamlDotNet.Serialization;

namespace Aspirate.Shared.Models.Compose;

public class ComposeBuildSecret
{
    [YamlMember(Alias = Aspirate.Shared.Literals.AspirateSecretLiterals.EnvironmentSecretsManagerLong)]
    public string? Environment { get; set; }

    [YamlMember(Alias = Aspirate.Shared.Literals.AspirateSecretLiterals.FileSecretsManager)]
    public string? File { get; set; }
}
