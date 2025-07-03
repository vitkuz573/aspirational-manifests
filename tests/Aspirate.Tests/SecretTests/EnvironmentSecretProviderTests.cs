using System;
using Xunit;

namespace Aspirate.Tests.SecretTests;

public class EnvironmentSecretProviderTests
{
    [Fact]
    public void AddSecret_SetsEnvironmentVariable()
    {
        var provider = new EnvironmentSecretProvider();
        provider.AddSecret("res", "key", "value");
        Assert.Equal("value", Environment.GetEnvironmentVariable("RES_KEY"));
    }

    [Fact]
    public void GetSecret_GetsEnvironmentVariable()
    {
        Environment.SetEnvironmentVariable("RES_KEY", "value");
        var provider = new EnvironmentSecretProvider();
        Assert.Equal("value", provider.GetSecret("res", "key"));
    }
}
