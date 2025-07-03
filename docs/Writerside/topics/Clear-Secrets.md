# Clearing Secrets

The `clear-secrets` command removes any stored secret state for the current project. When using the default file provider this deletes `aspirate-state.json`.

```bash
aspirate clear-secrets
```

## Cli Options (Optional)

| Option | Alias | Environmental Variable Counterpart | Description |
|-------|-------|------------------------------------|-------------|
| --force | -f | `ASPIRATE_FORCE` | Skip the confirmation prompt. Required for non‑interactive mode. |
| --non-interactive | | `ASPIRATE_NON_INTERACTIVE` | Disables interactive mode for the command |
| --secret-password | | `ASPIRATE_SECRET_PASSWORD` | Password used for secret operations when running non‑interactively |
| --pbkdf2-iterations | | `ASPIRATE_PBKDF2_ITERATIONS` | Override the PBKDF2 iteration count used for password hashing |
| --state-path | | `ASPIRATE_STATE_PATH` | Path to the directory containing `aspirate-state.json` |
| --disable-secrets | | `ASPIRATE_DISABLE_SECRETS` | Disables secrets management features. |
| --secret-provider | | `ASPIRATE_SECRET_PROVIDER` | Secret provider: `file`, `env` or `base64`. |
