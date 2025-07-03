namespace Aspirate.Secrets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecretProtectionStrategies(this IServiceCollection services) =>
        services
            .AddSingleton<ISecretProtectionStrategy, ConnectionStringProtector>()
            .AddSingleton<ISecretProtectionStrategy, PostgresPasswordProtector>()
            .AddSingleton<ISecretProtectionStrategy, MsSqlPasswordProtector>()
            .AddSingleton<ISecretProtectionStrategy, ApiKeyProtector>()
            .AddSingleton<ISecretProtectionStrategy, ClientSecretProtector>();

    public static IServiceCollection AddAspirateSecretProvider(this IServiceCollection services) =>
        services
            .AddSingleton<SecretProvider>()
            .AddSingleton<AzureKeyVaultSecretProvider>()
            .AddSingleton<SecretProviderFactory>()
            .AddSingleton<ISecretProvider, DelegatingSecretProvider>();
}
