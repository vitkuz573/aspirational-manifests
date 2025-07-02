# Secret Management

Aspirate now includes built-in support for robust secret management, allowing you to easily encrypt sensitive data such as connection strings.
This feature is designed to increase security and minimize vulnerabilities.

Aspir8 which uses AesGcm encryption to encrypt the secret's file using a password.
The user supplies this password during the `generate` and `apply` processes.
It's generated using Pbkdf2 with SHA256, one million iterations, and the hash and salt are stored in the secret file.
Secrets protected by this provider are only accessible to users who know the password, and are completely safe to store in a Git repository.

## Selecting a Secret Provider

Secrets can be stored locally (the default) or in Azure Key Vault. Use the
`--secret-provider` option or the `ASPIRATE_SECRET_PROVIDER` environment
variable to choose between `file` and `keyvault` backends.
