namespace Aspirate.Secrets;

public class AesGcmCrypter : IEncrypter, IDecrypter
{
    private const int NonceSize = 12;
    private readonly byte[] _key;
    private readonly int _tagSizeInBytes;

    public AesGcmCrypter(byte[] key, int tagSizeInBytes)
    {
        _key = key;
        _tagSizeInBytes = tagSizeInBytes;
    }

    public string EncryptValue(string plaintext)
    {
        using var aesGcm = new AesGcm(_key, _tagSizeInBytes);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = new byte[plaintextBytes.Length];
        var tag = new byte[_tagSizeInBytes];
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

        // Prepend the nonce and the tag to the ciphertext
        var resultBytes = new byte[NonceSize + tag.Length + ciphertextBytes.Length];
        Buffer.BlockCopy(nonce, 0, resultBytes, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, resultBytes, NonceSize, tag.Length);
        Buffer.BlockCopy(ciphertextBytes, 0, resultBytes, NonceSize + tag.Length, ciphertextBytes.Length);

        return Convert.ToBase64String(resultBytes);
    }

    public string DecryptValue(string ciphertext)
    {
        using var aesGcm = new AesGcm(_key, _tagSizeInBytes);

        var ciphertextBytes = Convert.FromBase64String(ciphertext);

        // Extract the nonce and the tag from the ciphertext
        var nonce = new byte[NonceSize];
        var tag = new byte[_tagSizeInBytes];
        var actualCiphertextBytes = new byte[ciphertextBytes.Length - NonceSize - tag.Length];
        Buffer.BlockCopy(ciphertextBytes, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(ciphertextBytes, NonceSize, tag, 0, tag.Length);
        Buffer.BlockCopy(ciphertextBytes, NonceSize + tag.Length, actualCiphertextBytes, 0, actualCiphertextBytes.Length);

        var plaintextBytes = new byte[actualCiphertextBytes.Length];
        aesGcm.Decrypt(nonce, actualCiphertextBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public Dictionary<string, string> BulkDecrypt(List<string> ciphertexts) =>
        ciphertexts.ToDictionary(ciphertext => ciphertext, DecryptValue);
}
