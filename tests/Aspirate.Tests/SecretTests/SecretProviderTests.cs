using System.Collections.Generic;
using System.Security.Cryptography;
using Xunit;

namespace Aspirate.Tests.SecretTests;

public class SecretProviderTests
{
    private const string TestPassword = "testPassword";
    private const string Base64Salt = "dxaPu37gk4KtgYBy";
    private const string TestKey = "testKey";
    private const string TestResource = "testresource";
    private const string DecryptedTestValue = "testValue";
    private const string EncryptedTestValue = "lbD6eicWrRyb6o+4mBDfDUJsrlOR0rstWPfPLr0nAg8OEAer3g==";
    private readonly IFileSystem _fileSystem = CreateMockFilesystem();

    [Fact]
    public void SetPassword_ShouldSetHash()
    {
        var provider = new SecretProvider(_fileSystem);
        provider.SetPassword(TestPassword);

        provider.State.Hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CheckPassword_ShouldMatchHash()
    {
        var provider = new SecretProvider(_fileSystem);
        provider.SetPassword(TestPassword);
        provider.CheckPassword(TestPassword).Should().BeTrue();
    }

    [Fact]
    public void SetPassword_RespectsIterationConfiguration()
    {
        var provider = new SecretProvider(_fileSystem) { Pbkdf2Iterations = 5 };
        var state = GetState(Base64Salt);
        provider.LoadState(state);

        provider.SetPassword(TestPassword);

        using var pbkdf2 = new Rfc2898DeriveBytes(TestPassword, Convert.FromBase64String(Base64Salt), 5, HashAlgorithmName.SHA256);
        var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));

        provider.State.Hash.Should().Be(expected);
    }

    [Fact]
    public void CheckPassword_RespectsIterationConfiguration()
    {
        var provider = new SecretProvider(_fileSystem) { Pbkdf2Iterations = 5 };
        var state = GetState(Base64Salt);
        provider.LoadState(state);
        provider.SetPassword(TestPassword);

        provider.CheckPassword(TestPassword).Should().BeTrue();

        provider.Pbkdf2Iterations = 6;

        provider.CheckPassword(TestPassword).Should().BeFalse();
    }

    [Fact]
    public void SecretState_ShouldExist()
    {
        var provider = new SecretProvider(_fileSystem);
        var state = GetState();
        provider.SecretStateExists(state).Should().BeTrue();
    }

    [Fact]
    public void SecretState_ShouldNotExist()
    {
        var provider = new SecretProvider(_fileSystem);
        var state = new AspirateState();
        provider.SecretStateExists(state).Should().BeFalse();
    }

    [Fact]
    public void RestoreState_ShouldSetState()
    {
        var provider = new SecretProvider(_fileSystem);
        var state = GetState();
        provider.LoadState(state);
        provider.State.Should().NotBeNull();
        provider.State.Salt.Should().BeNull();
    }

    [Fact]
    public void AddSecret_ShouldAddEncryptedSecretToState()
    {
        var provider = new SecretProvider(_fileSystem);
        var state = GetState(Base64Salt);
        provider.LoadState(state);
        provider.SetPassword(TestPassword);

        provider.AddResource(TestResource);
        provider.AddSecret(TestResource, TestKey, DecryptedTestValue);

        provider.State.Secrets[TestResource].Keys.Should().Contain(TestKey);
    }

    [Fact]
    public void RemoveSecret_ShouldRemoveSecretFromState()
    {
        var provider = new SecretProvider(_fileSystem);

        var state = GetState(
            Base64Salt, secrets: new()
            {
               [TestResource] = new()
               {
                   [TestKey] = EncryptedTestValue,
               },
            });

        provider.LoadState(state);

        provider.SetPassword(TestPassword);

        provider.RemoveSecret(TestResource, TestKey);

        provider.State.Secrets[TestResource].Keys.Should().NotContain(TestKey);
    }

    [Fact]
    public void GetSecret_ShouldReturnDecryptedSecret()
    {
        var provider = new SecretProvider(_fileSystem);

        var state = GetState(
            Base64Salt, secrets: new()
            {
                [TestResource] = new()
                {
                    [TestKey] = EncryptedTestValue,
                },
            });

        provider.LoadState(state);

        provider.SetPassword(TestPassword);

        var secret = provider.GetSecret(TestResource, TestKey);

        secret.Should().Be(DecryptedTestValue);
    }

    [Fact]
    public void RotatePassword_ShouldReEncryptSecrets()
    {
        var provider = new SecretProvider(_fileSystem);

        var state = GetState(
            Base64Salt,
            new Dictionary<string, Dictionary<string, string>>
            {
                [TestResource] = new()
                {
                    [TestKey] = EncryptedTestValue,
                },
            });

        provider.LoadState(state);
        provider.SetPassword(TestPassword);

        var original = provider.GetSecret(TestResource, TestKey);

        provider.RotatePassword("newPassword");

        provider.CheckPassword("newPassword").Should().BeTrue();
        var rotated = provider.GetSecret(TestResource, TestKey);

        rotated.Should().Be(original);
    }

    private static AspirateState GetState(string? salt = null, Dictionary<string, Dictionary<string, string>>? secrets = null)
    {
        var state = new SecretState
        {
            Salt = salt,
            Secrets = secrets ?? [],
        };

        return new AspirateState { SecretState = state };
    }

    private static MockFileSystem CreateMockFilesystem()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory($"/some-path/{AspirateLiterals.DefaultArtifactsPath}");
        fileSystem.Directory.SetCurrentDirectory($"/some-path");

        return fileSystem;
    }
}
