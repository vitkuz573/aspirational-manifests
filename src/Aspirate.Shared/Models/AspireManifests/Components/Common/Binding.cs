namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

/// <summary>
/// A binding for a project.
/// </summary>

[ExcludeFromCodeCoverage]
public class Binding
{
    private string? _scheme = BindingLiterals.Http;
    private string? _protocol = BindingLiterals.Tcp;
    private string? _transport;

    /// <summary>
    /// The scheme of the binding.
    /// </summary>
    [JsonPropertyName("scheme")]
    public string? Scheme
    {
        get => _scheme;
        set => _scheme = Validate(value, nameof(Scheme));
    }

    /// <summary>
    /// The protocol of the binding.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string? Protocol
    {
        get => _protocol;
        set => _protocol = Validate(value, nameof(Protocol));
    }

    /// <summary>
    /// The transport for the binding.
    /// </summary>
    [JsonPropertyName("transport")]
    public string? Transport
    {
        get => _transport;
        set => _transport = Validate(value, nameof(Transport));
    }

    /// <summary>
    /// The Container Port for the binding.
    /// </summary>
    [JsonPropertyName("port")]
    public int? Port { get; set; }

    /// <summary>
    /// The Container Port for the binding.
    /// </summary>
    [JsonPropertyName("targetPort")]
    public int? TargetPort { get; set; }

    /// <summary>
    /// Is External.
    /// </summary>
    [JsonPropertyName("external")]
    public bool External { get; set; }

    private static string? Validate(string? value, string propertyName)
    {
        if (value is null)
        {
            return null;
        }

        if (!BindingLiterals.ValidValues.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            var allowed = string.Join("', '", BindingLiterals.ValidValues);
            throw new InvalidOperationException($"Binding {propertyName} must be one of: '{allowed}'.");
        }

        return value;
    }
}
