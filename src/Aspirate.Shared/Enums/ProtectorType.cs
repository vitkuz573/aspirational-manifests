namespace Aspirate.Shared.Enums;

public class ProtectorType(string name, string value) : SmartEnum<ProtectorType, string>(name, value)
{
    public static readonly ProtectorType ConnectionString = new(nameof(ConnectionString), "ConnectionString");
    public static readonly ProtectorType PostgresPassword = new(nameof(PostgresPassword), "POSTGRES_PASSWORD");
    public static readonly ProtectorType MsSqlPassword = new(nameof(MsSqlPassword), "MSSQL_SA_PASSWORD");
    public static readonly ProtectorType ApiKey = new(nameof(ApiKey), "API_KEY");
    public static readonly ProtectorType ClientSecret = new(nameof(ClientSecret), "CLIENT_SECRET");
}
