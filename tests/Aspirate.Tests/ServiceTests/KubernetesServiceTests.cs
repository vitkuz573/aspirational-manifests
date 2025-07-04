using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ServiceTests;

public class KubernetesServiceTests : BaseServiceTests<IKubernetesService>
{
    [Fact]
    public void ConvertResourcesToKubeObjects_WithPrivateRegistry_AddsImagePullSecret()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(fs.Path.GetTempPath());

        var state = CreateAspirateState(nonInteractive: true, password: "pwd");
        state.WithPrivateRegistry = true;

        var secretProvider = new SecretProvider(fs)
        {
            State = new SecretState()
        };

        secretProvider.SetPassword("pwd");
        secretProvider.AddResource(TemplateLiterals.ImagePullSecretType);
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryUrl", "https://registry.example.com");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryUsername", "user");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryPassword", "pass");
        secretProvider.AddSecret(TemplateLiterals.ImagePullSecretType, "registryEmail", "user@example.com");
        secretProvider.SetState(state);
        secretProvider.ClearPassword();

        var provider = CreateServiceProvider(state, new TestConsole(), fs, secretProvider);
        var sut = provider.GetRequiredService<IKubernetesService>();

        var result = sut.ConvertResourcesToKubeObjects(new List<KeyValuePair<string, Resource>>(), state, false);

        var secret = result.OfType<V1Secret>().FirstOrDefault();
        secret.Should().NotBeNull();
        secret!.Metadata.Name.Should().Be(TemplateLiterals.ImagePullSecretType);
    }
}

