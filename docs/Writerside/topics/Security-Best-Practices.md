# Security Best Practices

- Never commit the password used to encrypt secrets.
- Restrict access to the state file as described in [File Permissions](File-Permissions.md).
- Rotate your encryption password regularly using the [rotate-password](Rotate-Password.md) command.
- Prefer an external provider such as Azure Key Vault for shared environments.
