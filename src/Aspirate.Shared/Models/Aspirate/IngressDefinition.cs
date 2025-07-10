namespace Aspirate.Shared.Models.Aspirate;

/// <summary>
/// Represents ingress configuration for a resource.
/// </summary>
public sealed class IngressDefinition
{
    /// <summary>
    /// Host names for the ingress rule.
    /// </summary>
    public List<string> Hosts { get; set; } = [];

    /// <summary>
    /// Path for the ingress rule. Defaults to '/'.
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Optional TLS secret name.
    /// </summary>
    public string? TlsSecret { get; set; }

    /// <summary>
    /// Optional service port number to use for the ingress backend.
    /// </summary>
    public int? PortNumber { get; set; }

    /// <summary>
    /// Optional annotations to apply to the ingress resource.
    /// </summary>
    public Dictionary<string, string>? Annotations { get; set; }
}
