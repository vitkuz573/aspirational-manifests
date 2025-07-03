namespace Aspirate.Secrets;

public class SecretProvider(IFileSystem fileSystem) : ISecretProvider
{
    private const int TagSizeInBytes = 16;
    private const int DefaultIterations = SecretState.DefaultIterations;
    private const int MinimumIterations = 100_000;

    private int _pbkdf2Iterations = DefaultIterations;
    public int Pbkdf2Iterations
    {
        get => _pbkdf2Iterations;
        set => _pbkdf2Iterations = value >= MinimumIterations
            ? value
            : throw new ArgumentOutOfRangeException(
                nameof(Pbkdf2Iterations),
                value,
                $"Iterations must be at least {MinimumIterations}");
    }
    private SecureString? _password;
    private IEncrypter? _encrypter;
    private IDecrypter? _decrypter;
    private byte[]? _salt;
    public SecretState? State { get; set; }

    public void SetPassword(string password)
    {
        _password?.Dispose();
        _password = new SecureString();
        foreach (var ch in password)
        {
            _password.AppendChar(ch);
        }
        _password.MakeReadOnly();

        if (_salt is null)
        {
            CreateNewSalt();
        }

        // Derive a key from the passphrase using Pbkdf2 with SHA256.
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt: _salt, iterations: Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(32); // 256 bit key

        if (State?.SecretsVersion == 1)
        {
            var crypter = new AesGcmCrypter(key, TagSizeInBytes);
            _encrypter = crypter;
            _decrypter = crypter;
        }
        else
        {
            var crypter = new AesCbcCrypter(key);
            _encrypter = crypter;
            _decrypter = crypter;
        }

        SetPasswordHash();
        if (State is not null)
        {
            State.Pbkdf2Iterations = Pbkdf2Iterations;
        }
    }

    public bool CheckPassword(string password)
    {
        using var pbkdf2ToCheck = new Rfc2898DeriveBytes(password, salt: _salt, iterations: Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var passwordToCheckHash = Convert.ToBase64String(pbkdf2ToCheck.GetBytes(32));

        return passwordToCheckHash == State.Hash;
    }

    public void ProcessAfterStateRestoration()
    {
        if (_password?.Length > 0)
        {
            ClearPassword();
        }

        State ??= new();

        if (State.SecretsVersion == 0)
        {
            State.SecretsVersion = SecretState.CurrentVersion;
        }

        _salt = !string.IsNullOrEmpty(State.Salt) ? Convert.FromBase64String(State.Salt) : null;
        Pbkdf2Iterations = State.Pbkdf2Iterations > 0 ? State.Pbkdf2Iterations : DefaultIterations;
    }

    private void CreateNewSalt()
    {
        _salt = new byte[12];
        RandomNumberGenerator.Fill(_salt);
        State ??= new();
        if (State.SecretsVersion == 0)
        {
            State.SecretsVersion = SecretState.CurrentVersion;
        }
        State.Salt = Convert.ToBase64String(_salt);
        State.Pbkdf2Iterations = Pbkdf2Iterations;
    }

    private void SetPasswordHash()
    {
        var pwd = ConvertToUnsecureString(_password!);
        try
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(pwd, salt: _salt, iterations: Pbkdf2Iterations, HashAlgorithmName.SHA256);
            State.Hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        }
        finally
        {
            pwd = string.Empty;
        }
    }

    private static string ConvertToUnsecureString(SecureString secureString)
    {
        var ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
        try
        {
            return Marshal.PtrToStringUni(ptr)!;
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

     protected readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void AddSecret(string resourceName, string key, string value)
    {
        if (State?.Secrets == null || _encrypter == null)
        {
            return;
        }

        var protectedValue = _encrypter?.EncryptValue(value);
        State.Secrets[resourceName][key] = protectedValue;
    }

    public void RemoveSecret(string resourceName, string key) =>
        State?.Secrets[resourceName].Remove(key);

    public bool ResourceExists(string resourceName) => State?.Secrets.TryGetValue(resourceName, out _) == true;
    public bool SecretExists(string resourceName, string key) => State?.Secrets[resourceName].TryGetValue(key, out _) == true;

    public void RemoveResource(string resourceName) =>
        State?.Secrets.Remove(resourceName);

    public void AddResource(string resourceName) =>
        State?.Secrets.Add(resourceName, []);

    public void SetState(AspirateState state)
    {
        if (State == null)
        {
            return;
        }

        state.SecretState = State;
    }

    public void LoadState(AspirateState state)
    {
        State = state.SecretState;

        ProcessAfterStateRestoration();
    }

    public void RemoveState(AspirateState state)
    {
        State = null;
        state.SecretState = null;

        ProcessAfterStateRestoration();
    }

    public bool SecretStateExists(AspirateState state) => state.SecretState != null;

    public string? GetSecret(string resourceName, string key)
    {
        if (State?.Secrets == null || _decrypter == null)
        {
            return null;
        }

        return State.Secrets[resourceName].TryGetValue(key, out var encryptedValue) ? _decrypter.DecryptValue(encryptedValue) : null;
    }

    public void RotatePassword(string newPassword)
    {
        if (State?.Secrets == null || _decrypter == null)
        {
            return;
        }

        var decrypted = new Dictionary<string, Dictionary<string, string>>();

        foreach (var resource in State.Secrets)
        {
            decrypted[resource.Key] = new Dictionary<string, string>();

            foreach (var secret in resource.Value)
            {
                decrypted[resource.Key][secret.Key] = _decrypter.DecryptValue(secret.Value);
            }
        }

        _salt = null;
        SetPassword(newPassword);

        State.SecretsVersion = SecretState.CurrentVersion;

        State.Secrets = new Dictionary<string, Dictionary<string, string>>();

        foreach (var resource in decrypted)
        {
            State.Secrets[resource.Key] = new Dictionary<string, string>();

            foreach (var secret in resource.Value)
            {
                State.Secrets[resource.Key][secret.Key] = _encrypter!.EncryptValue(secret.Value);
            }
        }
    }

    public void UpgradeEncryption()
    {
        if (_password == null || State?.Secrets == null || _decrypter == null)
        {
            return;
        }

        var decrypted = new Dictionary<string, Dictionary<string, string>>();

        foreach (var resource in State.Secrets)
        {
            decrypted[resource.Key] = new Dictionary<string, string>();

            foreach (var secret in resource.Value)
            {
                decrypted[resource.Key][secret.Key] = _decrypter.DecryptValue(secret.Value);
            }
        }

        _salt = null;
        State.SecretsVersion = SecretState.CurrentVersion;
        SetPassword(ConvertToUnsecureString(_password));

        State.Secrets = new Dictionary<string, Dictionary<string, string>>();

        foreach (var resource in decrypted)
        {
            State.Secrets[resource.Key] = new Dictionary<string, string>();

            foreach (var secret in resource.Value)
            {
                State.Secrets[resource.Key][secret.Key] = _encrypter!.EncryptValue(secret.Value);
            }
        }
    }

    public void ClearPassword()
    {
        if (_password != null)
        {
            _password.Dispose();
            _password = null;
        }

        _encrypter = null;
        _decrypter = null;
    }
}
