namespace Aspirate.Shared.Literals;

[ExcludeFromCodeCoverage]
public static class BindingLiterals
{
    public const string Http = "http";
    public const string Https = "https";
    public const string Tcp = "tcp";
    public const string Udp = "udp";
    public const string Http2 = "http2";

    public static readonly string[] ValidSchemes = [Http, Https, Tcp, Udp];

    public static readonly string[] ValidProtocols = [Tcp, Udp];

    public static readonly string[] ValidTransports = [Http, Http2, Tcp];
}
