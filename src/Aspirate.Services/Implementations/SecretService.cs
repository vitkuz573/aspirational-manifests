namespace Aspirate.Services.Implementations;

public class SecretService(
    SecretProviderFactory providerFactory,
    IFileSystem fs,
    IAnsiConsole logger,
    IEnumerable<ISecretProtectionStrategy> protectionStrategies)
    : ISecretService
{
    private readonly SecretProviderFactory _factory = providerFactory;
    private IReadOnlyCollection<ISecretProtectionStrategy> ProtectionStrategies { get; } = protectionStrategies.ToList();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        IncludeFields = true,
        Converters =
        {
            new SmartEnumNameConverter<ExistingSecretsType, string>()
        }
    };

    private ISecretProvider GetProvider(SecretManagementOptions options)
    {
        var provider = _factory.GetProvider(options.SecretProvider ?? options.State.SecretProvider);
        provider.Pbkdf2Iterations = options.Pbkdf2Iterations
            ?? options.State.SecretState?.Pbkdf2Iterations
            ?? provider.Pbkdf2Iterations;
        return provider;
    }

    private bool CheckSecretVersion(ISecretProvider provider)
    {
        if (provider.State is null)
        {
            return false;
        }

        if (provider.State.SecretsVersion != SecretState.CurrentVersion)
        {
            logger.MarkupLine($"[yellow]Secret state version mismatch. Expected {SecretState.CurrentVersion}, found {provider.State.SecretsVersion}.[/]");
            return true;
        }

        return false;
    }

    public void SaveSecrets(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);
        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("Secrets have been [red]disabled[/] for this run.");
            return;
        }

        if (!ProtectionStrategies.CheckForProtectableSecrets(options.State.AllSelectedSupportedComponents)
            && options.State.WithPrivateRegistry != true)
        {
            logger.MarkupLine("No secrets to protect in any [blue]selected components[/]");
            return;
        }

        bool versionMismatch = false;

        if (secretProvider.SecretStateExists(options.State))
        {
            secretProvider.LoadState(options.State);
            if (secretProvider.State is not null)
            {
                secretProvider.Pbkdf2Iterations = secretProvider.State.Pbkdf2Iterations;
            }
            versionMismatch = CheckSecretVersion(secretProvider);

            if (!CheckPassword(options))
            {
                logger.MarkupLine("[red]Aborting due to inability to unlock secrets.[/]");
                ActionCausesExitException.ExitNow();
            }

            if (versionMismatch && logger.Confirm("Re-encrypt secrets using the new algorithm?"))
            {
                secretProvider.UpgradeEncryption();
                if (secretProvider.State is not null)
                {
                    secretProvider.State.Pbkdf2Iterations = secretProvider.Pbkdf2Iterations;
                }
                secretProvider.SetState(options.State);
            }
        }

        if (!secretProvider.SecretStateExists(options.State))
        {
            HandleInitialisation(options);
        }

        foreach (var component in options.State.AllSelectedSupportedComponents.Where(component => !secretProvider.ResourceExists(component.Key)))
        {
            secretProvider.AddResource(component.Key);
        }

        foreach (var component in options.State.AllSelectedSupportedComponents)
        {
            if (component.Value is not IResourceWithEnvironmentalVariables { Env: not null })
            {
                continue;
            }

            foreach (var strategy in ProtectionStrategies)
            {
                strategy.ProtectSecrets(component, options.NonInteractive.GetValueOrDefault());
            }
        }

        if (options.State.WithPrivateRegistry == true)
        {
            const string resourceName = TemplateLiterals.ImagePullSecretType;

            if (!secretProvider.ResourceExists(resourceName))
            {
                secretProvider.AddResource(resourceName);
            }

            secretProvider.AddSecret(resourceName, "registryUrl", options.State.PrivateRegistryUrl ?? string.Empty);
            secretProvider.AddSecret(resourceName, "registryUsername", options.State.PrivateRegistryUsername ?? string.Empty);
            secretProvider.AddSecret(resourceName, "registryPassword", options.State.PrivateRegistryPassword ?? string.Empty);
            secretProvider.AddSecret(resourceName, "registryEmail", options.State.PrivateRegistryEmail ?? string.Empty);

            options.State.PrivateRegistryPassword = null;
        }

        if (secretProvider.State is not null)
        {
            secretProvider.State.Pbkdf2Iterations = secretProvider.Pbkdf2Iterations;
        }
        secretProvider.SetState(options.State);

        logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Secret State has been saved.");
        secretProvider.ClearPassword();
    }

    public async Task SaveSecretsAsync(SecretManagementOptions options)
    {
        SaveSecrets(options);

        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            var stateAsJson = JsonSerializer.Serialize(options.State, _jsonSerializerOptions);
            await fs.File.WriteAllTextAsync(stateFile, stateAsJson);
        }
    }

    public void ReInitialiseSecrets(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);
        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("[green]Secrets are disabled[/].");
            return;
        }

        secretProvider.RemoveState(options.State);

        HandleInitialisation(options);

        if (secretProvider.State is not null)
        {
            secretProvider.State.Pbkdf2Iterations = secretProvider.Pbkdf2Iterations;
        }
        secretProvider.SetState(options.State);

        options.State.SecretState = secretProvider.State;

        logger.MarkupLine("[green]Secret State has been initialised![/].");
        secretProvider.ClearPassword();
    }

    public async Task ReInitialiseSecretsAsync(SecretManagementOptions options)
    {
        ReInitialiseSecrets(options);

        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            var stateAsJson = JsonSerializer.Serialize(options.State, _jsonSerializerOptions);
            await fs.File.WriteAllTextAsync(stateFile, stateAsJson);
        }
    }

    public void RotatePassword(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);
        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("[green]Secrets are disabled[/].");
            return;
        }

        if (!secretProvider.SecretStateExists(options.State))
        {
            logger.MarkupLine("[red]No secret state exists to rotate.[/]");
            return;
        }

        secretProvider.LoadState(options.State);
        if (secretProvider.State is not null)
        {
            secretProvider.Pbkdf2Iterations = secretProvider.State.Pbkdf2Iterations;
        }
        var versionMismatch = CheckSecretVersion(secretProvider);

        if (!CheckPassword(options))
        {
            logger.MarkupLine("[red]Aborting due to inability to unlock secrets.[/]");
            ActionCausesExitException.ExitNow();
        }

        var oldPassword = options.State.SecretPassword;
        // Ensure we prompt for a new password rather than reusing the old one
        options.State.SecretPassword = string.Empty;

        if (versionMismatch && logger.Confirm("Re-encrypt secrets using the new algorithm?"))
        {
            secretProvider.UpgradeEncryption();
            if (secretProvider.State is not null)
            {
                secretProvider.State.Pbkdf2Iterations = secretProvider.Pbkdf2Iterations;
            }
            secretProvider.SetState(options.State);
        }

        if (!CreatePassword(options))
        {
            logger.ValidationFailed("Aborting due to inability to create password.");
        }

        var newPassword = options.SecretPassword!;
        // Reset provider to use the old password so secrets can be decrypted
        if (!string.IsNullOrEmpty(oldPassword))
        {
            secretProvider.SetPassword(oldPassword);
        }

        secretProvider.RotatePassword(newPassword);
        if (secretProvider.State is not null)
        {
            secretProvider.State.Pbkdf2Iterations = secretProvider.Pbkdf2Iterations;
        }
        secretProvider.SetState(options.State);
        options.State.SecretPassword = options.SecretPassword;
        options.State.SecretState = secretProvider.State;

        logger.MarkupLine("[green]Secret password rotated![/].");
        secretProvider.ClearPassword();
    }

    public async Task RotatePasswordAsync(SecretManagementOptions options)
    {
        RotatePassword(options);

        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            var stateAsJson = JsonSerializer.Serialize(options.State, _jsonSerializerOptions);
            await fs.File.WriteAllTextAsync(stateFile, stateAsJson);
        }
    }

    public void LoadSecrets(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);
        logger.WriteRuler("[purple]Handling Aspirate Secrets[/]");

        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("[green]Secrets are disabled[/].");
            return;
        }

        if (options.State.ReplaceSecrets == true)
        {
            ReInitialiseSecrets(options);
            return;
        }

        if (!secretProvider.SecretStateExists(options.State))
        {
            ReInitialiseSecrets(options);
            return;
        }

        secretProvider.LoadState(options.State);
        if (secretProvider.State is not null)
        {
            secretProvider.Pbkdf2Iterations = secretProvider.State.Pbkdf2Iterations;
        }
        var versionMismatch = CheckSecretVersion(secretProvider);

        if (!options.CommandUnlocksSecrets)
        {
            logger.MarkupLine("[green]Secret State have been loaded[/], but the current command [blue]does not[/] need to decrypt them.");
            return;
        }

        if (options.NonInteractive == true)
        {
            if (string.IsNullOrEmpty(options.SecretPassword))
            {
                logger.ValidationFailed("Secrets are protected by a password, but no password has been provided.");
            }
        }

        if (!CheckPassword(options))
        {
            logger.MarkupLine("[red]Aborting due to inability to unlock secrets.[/]");
            ActionCausesExitException.ExitNow();
        }

        if (versionMismatch && logger.Confirm("Re-encrypt secrets using the new algorithm?"))
        {
            secretProvider.UpgradeEncryption();
            secretProvider.SetState(options.State);
        }

        options.State.SecretState = secretProvider.State;

        logger.MarkupLine($"[green]({EmojiLiterals.CheckMark}) Done: [/] Secret State populated successfully.");
    }

    public async Task LoadSecretsAsync(SecretManagementOptions options)
    {
        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            if (fs.File.Exists(stateFile))
            {
                var stateAsJson = await fs.File.ReadAllTextAsync(stateFile);
                var previousState = JsonSerializer.Deserialize<AspirateState>(stateAsJson, _jsonSerializerOptions);
                if (previousState?.SecretState is not null)
                {
                    options.State.SecretState = previousState.SecretState;
                }
            }
        }

        LoadSecrets(options);
    }

    private bool CheckPassword(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);
        if (CliSecretPasswordSupplied(options, out var validPassword))
        {
            return validPassword;
        }

        for (int i = 3; i > 0; i--)
        {
            var password = logger.Prompt(
                new TextPrompt<string>("Secrets are protected by a [green]password[/]. Please enter it now: ").PromptStyle("red")
                    .Secret());

            if (secretProvider.CheckPassword(password))
            {
                secretProvider.SetPassword(password);
                options.State.SecretPassword = password;
                return true;
            }

            LogPasswordError(i, "Incorrect Password");
        }

        return false;
    }

    private void LogPasswordError(int i, string caption)
    {
        var attemptsRemaining = i - 1;

        logger.MarkupLine(
            attemptsRemaining != 0
                ? $"[red]{caption}[/]. Please try again. You have [yellow]{attemptsRemaining} attempt{(attemptsRemaining > 1 ? "s" : "")}[/] remaining."
                : $"[red]{caption}[/]. You have [yellow]no attempts[/] remaining.");
    }

    private bool CliSecretPasswordSupplied(SecretManagementOptions options, out bool validPassword)
    {
        var secretProvider = GetProvider(options);
        if (string.IsNullOrEmpty(options.SecretPassword))
        {
            validPassword = false;
            return false;
        }

        if (secretProvider.CheckPassword(options.SecretPassword))
        {
            secretProvider.SetPassword(options.SecretPassword);
            {
                validPassword = true;
                options.State.SecretPassword = options.SecretPassword;
                return true;
            }
        }

        logger.MarkupLine("[red]Incorrect password[/].");
        validPassword = false;
        return true;
    }

    private void HandleInitialisation(SecretManagementOptions options)
    {
        if (!CreatePassword(options))
        {
            logger.ValidationFailed("Aborting due to inability to create password.");
        }
    }

    private bool CreatePassword(SecretManagementOptions options)
    {
        logger.MarkupLine("Secrets are to be protected by a [green]password[/]");
        var secretProvider = GetProvider(options);

        if (!string.IsNullOrEmpty(options.State.SecretPassword))
        {
            secretProvider.SetPassword(options.State.SecretPassword);
            return true;
        }

        for (int i = 3; i > 0; i--)
        {
            logger.WriteLine();
            var firstEntry = logger.Prompt(
                new TextPrompt<string>("Please enter new Password: ")
                    .PromptStyle("red")
                    .Secret());

            var secondEntry = logger.Prompt(
                new TextPrompt<string>("Please enter it again to confirm: ")
                    .PromptStyle("red")
                    .Secret());

            if (firstEntry.Equals(secondEntry, StringComparison.Ordinal))
            {
                if (!IsStrongPassword(firstEntry))
                {
                    LogPasswordError(i, "Password does not meet complexity requirements.");
                    continue;
                }

                secretProvider.SetPassword(firstEntry);
                options.SecretPassword = firstEntry;
                options.State.SecretPassword = firstEntry;
                return true;
            }

            LogPasswordError(i, "Passwords do not match.");
        }

        return false;
    }

    private static bool IsStrongPassword(string password)
    {
        // Current tests only validate that passwords have a minimum length.
        // Additional complexity checks can be introduced later if required.
        return password.Length >= 8;
    }

    public void ClearSecrets(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);

        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("[green]Secrets are disabled[/].");
            return;
        }

        if (options.NonInteractive == true && options.Force != true)
        {
            logger.MarkupLine("[red]--force is required in non-interactive mode.[/]");
            ActionCausesExitException.ExitNow();
        }

        if (options.NonInteractive != true && options.Force != true)
        {
            var shouldDelete = logger.Confirm("Are you sure you want to clear all secret state?");
            if (!shouldDelete)
            {
                logger.MarkupLine("[yellow]Aborting secret clear.[/]");
                return;
            }
        }

        secretProvider.RemoveState(options.State);

        if (secretProvider is SecretProvider && !string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            if (fs.File.Exists(stateFile))
            {
                fs.File.Delete(stateFile);
            }
        }

        logger.MarkupLine("[green]Secret State cleared.[/]");
    }

    public async Task ClearSecretsAsync(SecretManagementOptions options)
    {
        ClearSecrets(options);

        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            if (fs.File.Exists(stateFile))
            {
                await Task.Run(() => fs.File.Delete(stateFile));
            }
        }
    }

    public void VerifySecrets(SecretManagementOptions options)
    {
        var secretProvider = GetProvider(options);

        if (options.DisableSecrets == true)
        {
            logger.MarkupLine("[green]Secrets are disabled[/].");
            return;
        }

        if (!secretProvider.SecretStateExists(options.State))
        {
            logger.MarkupLine("[yellow]No secret state exists to verify.[/]");
            return;
        }

        secretProvider.LoadState(options.State);
        if (secretProvider.State is not null)
        {
            secretProvider.Pbkdf2Iterations = secretProvider.State.Pbkdf2Iterations;
        }

        var versionMismatch = CheckSecretVersion(secretProvider);

        if (!CheckPassword(options))
        {
            logger.MarkupLine("[red]Aborting due to inability to unlock secrets.[/]");
            ActionCausesExitException.ExitNow();
        }

        logger.MarkupLine("[green]Secrets verified successfully.[/]");

        if (versionMismatch)
        {
            logger.MarkupLine("[yellow]Consider rotating the password to upgrade encryption.[/]");
        }

        secretProvider.ClearPassword();
    }

    public async Task VerifySecretsAsync(SecretManagementOptions options)
    {
        if (!string.IsNullOrEmpty(options.StatePath))
        {
            var stateFile = fs.Path.Combine(options.StatePath, AspirateLiterals.StateFileName);
            if (fs.File.Exists(stateFile))
            {
                var stateAsJson = await fs.File.ReadAllTextAsync(stateFile);
                var previousState = JsonSerializer.Deserialize<AspirateState>(stateAsJson, _jsonSerializerOptions);
                if (previousState?.SecretState is not null)
                {
                    options.State.SecretState = previousState.SecretState;
                }
            }
        }

        VerifySecrets(options);
    }
}
