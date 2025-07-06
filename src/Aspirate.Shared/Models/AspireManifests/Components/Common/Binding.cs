namespace Aspirate.Shared.Models.AspireManifests.Components.Common;

/// <summary>
/// A binding for a project.
/// </summary>

[ExcludeFromCodeCoverage]
public class Binding : IJsonOnDeserialized
{
    private string? _scheme;
    private string? _protocol;
    private string? _transport;

    /// <summary>
    /// The scheme of the binding.
    /// </summary>
    [JsonPropertyName("scheme")]
    public string? Scheme
    {
        get => _scheme;
        set => _scheme = Validate(value, nameof(Scheme), BindingLiterals.ValidSchemes);
    }

    /// <summary>
    /// The protocol of the binding.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string? Protocol
    {
        get => _protocol;
        set => _protocol = Validate(value, nameof(Protocol), BindingLiterals.ValidProtocols);
    }

    /// <summary>
    /// The transport for the binding.
    /// </summary>
    [JsonPropertyName("transport")]
    public string? Transport
    {
        get => _transport;
        set => _transport = Validate(value, nameof(Transport), BindingLiterals.ValidTransports);
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

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (string.IsNullOrWhiteSpace(_scheme))
        {
            throw new InvalidOperationException("Scheme is required for a binding.");
        }

        if (string.IsNullOrWhiteSpace(_protocol))
        {
            throw new InvalidOperationException("Protocol is required for a binding.");
        }

        if (string.IsNullOrWhiteSpace(_transport))
        {
            throw new InvalidOperationException("Transport is required for a binding.");
        }
    }

    private static string? Validate(string? value, string propertyName, IReadOnlyCollection<string> validValues)
    {
        if (value is null)
        {
            return null;
        }

        if (!validValues.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            var allowed = string.Join("', '", validValues);
            throw new InvalidOperationException($"Binding {propertyName} must be one of: '{allowed}'.");
        }

        return value;
    }
}
