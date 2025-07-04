namespace Aspirate.Shared.Models.Aspirate;

/// <summary>
/// Represents ingress configuration for a resource.
/// </summary>
public sealed class IngressDefinition
{
    /// <summary>
    /// Host name for the ingress rule.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Path for the ingress rule. Defaults to '/'.
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Optional TLS secret name.
    /// </summary>
    public string? TlsSecret { get; set; }
}
