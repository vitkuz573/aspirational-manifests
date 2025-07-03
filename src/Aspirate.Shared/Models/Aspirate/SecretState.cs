namespace Aspirate.Shared.Models.Aspirate;

public sealed class SecretState
{
public const int CurrentVersion = 2;

    [JsonPropertyName("salt")]
    [RestorableStateProperty]
    public string? Salt { get; set; }

    [JsonPropertyName("hash")]
    [RestorableStateProperty]
    public string? Hash { get; set; }

    [JsonPropertyName("secrets")]
    [RestorableStateProperty]
    public Dictionary<string, Dictionary<string, string>> Secrets { get; set; } = [];

    [JsonPropertyName("secretsVersion")]
    [RestorableStateProperty]
    public int SecretsVersion { get; set; } = CurrentVersion;
}
