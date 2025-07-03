# Security Best Practices

- Never commit the password used to encrypt secrets.
- Restrict access to the state file as described in [File Permissions](File-Permissions.md).
- Rotate your encryption password regularly using the [rotate-password](Rotate-Password.md) command.
- Remove stored secrets with the [clear-secrets](Clear-Secrets.md) command when they are no longer required.
- Prefer an external provider such as Azure Key Vault for shared environments.
- Avoid setting long-lived environment variables containing secret passwords. Supply passwords via prompts or temporary variables scoped to a single command.
- Aspirate clears passwords from memory after secret operations such as generate, apply, and rotate to minimize exposure.
- Tune the secret encryption strength using the `ASPIRATE_PBKDF2_ITERATIONS` configuration option if higher security is required.
