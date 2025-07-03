namespace Aspirate.Shared.Models.Aspirate;

public sealed class SecretState
{
    public const int CurrentVersion = 2;

    /// <summary>
    /// Default PBKDF2 iteration count used when none is specified.
    /// </summary>
    public const int DefaultIterations = 1_000_000;

    [JsonPropertyName("salt")]
    [RestorableStateProperty]
    public string? Salt { get; set; }

    [JsonPropertyName("hash")]
    [RestorableStateProperty]
    public string? Hash { get; set; }

    [JsonPropertyName("pbkdf2Iterations")]
    [RestorableStateProperty]
    public int Pbkdf2Iterations { get; set; } = DefaultIterations;

    [JsonPropertyName("secrets")]
    [RestorableStateProperty]
    public Dictionary<string, Dictionary<string, string>> Secrets { get; set; } = [];

    [JsonPropertyName("secretsVersion")]
    [RestorableStateProperty]
    public int SecretsVersion { get; set; } = CurrentVersion;
}
