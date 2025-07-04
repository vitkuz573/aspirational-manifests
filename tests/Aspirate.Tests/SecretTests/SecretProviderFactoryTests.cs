using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aspirate.Tests.SecretTests;

public class SecretProviderFactoryTests
{
    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<SecretProvider>();
        services.AddSingleton<Base64SecretProvider>();
        services.AddSingleton<EnvironmentSecretProvider>();
        services.AddSingleton<SecretProviderFactory>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GetProvider_File_ReturnsFileProvider()
    {
        var factory = CreateServices().GetRequiredService<SecretProviderFactory>();
        var provider = factory.GetProvider("file");
        Assert.IsType<SecretProvider>(provider);
    }

    [Fact]
    public void GetProvider_Env_ReturnsEnvironmentProvider()
    {
        var factory = CreateServices().GetRequiredService<SecretProviderFactory>();
        var provider = factory.GetProvider("env");
        Assert.IsType<EnvironmentSecretProvider>(provider);
    }

    [Fact]
    public void GetProvider_Base64_ReturnsBase64Provider()
    {
        var factory = CreateServices().GetRequiredService<SecretProviderFactory>();
        var provider = factory.GetProvider("base64");
        Assert.IsType<Base64SecretProvider>(provider);
    }

    [Fact]
    public void GetProvider_Unknown_Throws()
    {
        var factory = CreateServices().GetRequiredService<SecretProviderFactory>();
        Assert.Throws<InvalidOperationException>(() => factory.GetProvider("bad"));
    }
}
