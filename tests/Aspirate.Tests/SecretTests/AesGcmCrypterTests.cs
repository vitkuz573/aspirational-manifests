namespace Aspirate.Tests.SecretTests;

public class AesGcmCrypterTests
{
    [Fact]
    public void EncryptValue_SamePlaintextTwice_ShouldProduceDifferentCiphertext()
    {
        // Arrange
        var key = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
        var crypter = new AesGcmCrypter(key, 16);
        const string plaintext = "hello";

        // Act
        var ciphertext1 = crypter.EncryptValue(plaintext);
        var ciphertext2 = crypter.EncryptValue(plaintext);

        // Assert
        ciphertext1.Should().NotBe(ciphertext2);
    }

    [Fact]
    public void DecryptValue_WithStoredNonce_ReturnsOriginalPlaintext()
    {
        // Arrange
        var key = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
        var crypter = new AesGcmCrypter(key, 16);
        const string plaintext = "hello";

        // Act
        var ciphertext = crypter.EncryptValue(plaintext);
        var decrypted = crypter.DecryptValue(ciphertext);

        // Assert
        decrypted.Should().Be(plaintext);
    }
}
