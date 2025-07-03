using Xunit;

namespace Aspirate.Tests.SecretTests;

public class Base64SecretProviderTests
{
    [Fact]
    public void AddAndGetSecret_RoundTripsValue()
    {
        var provider = new Base64SecretProvider();
        provider.AddResource("res");
        provider.AddSecret("res", "key", "value");
        Assert.Equal("value", provider.GetSecret("res", "key"));
    }
}
