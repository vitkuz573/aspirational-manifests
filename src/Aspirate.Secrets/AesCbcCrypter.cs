using System.Security.Cryptography;
using System.Text;
using Aspirate.Shared.Interfaces.Secrets;

namespace Aspirate.Secrets;

public class AesCbcCrypter : IEncrypter, IDecrypter
{
    private const int IvSize = 16;
    private readonly byte[] _key;

    public AesCbcCrypter(byte[] key)
    {
        _key = key;
    }

    public string EncryptValue(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = _key;
        aes.GenerateIV();
        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    public string DecryptValue(string ciphertext)
    {
        var allBytes = Convert.FromBase64String(ciphertext);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = _key;
        var iv = new byte[IvSize];
        Buffer.BlockCopy(allBytes, 0, iv, 0, IvSize);
        aes.IV = iv;
        var cipherBytes = new byte[allBytes.Length - IvSize];
        Buffer.BlockCopy(allBytes, IvSize, cipherBytes, 0, cipherBytes.Length);
        var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public Dictionary<string, string> BulkDecrypt(List<string> ciphertexts) =>
        ciphertexts.ToDictionary(cipher => cipher, DecryptValue);
}
