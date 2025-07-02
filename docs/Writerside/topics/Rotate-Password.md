# Rotating the Password

The `rotate-password` command allows you to change the password used to encrypt secrets stored in `aspirate-state.json`.

When executed, you will be prompted for the current password protecting the secrets. After successful verification, you will be asked to enter a new password twice. All existing secrets will then be re–encrypted using the new password.

```bash
  aspirate rotate-password
```

## Cli Options (Optional)

| Option | Alias | Environmental Variable Counterpart | Description |
|-------|-------|------------------------------------|-------------|
| --secret-password |       | `ASPIRATE_SECRET_PASSWORD` | Supply the current password when running non‑interactively. |
| --non-interactive |       | `ASPIRATE_NON_INTERACTIVE` | Disables interactive mode for the command |
| --disable-secrets |       | `ASPIRATE_DISABLE_SECRETS` | Disables secrets management features. |
