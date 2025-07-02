# External Providers

Secrets do not have to be stored in the local file provider. Aspir8 supports Azure Key Vault as an alternative backend.

Choose a provider with the `--secret-provider` option or set `ASPIRATE_SECRET_PROVIDER`. When selecting `keyvault` you must also supply the vault name using `--keyvault-name` or `ASPIRATE_KEYVAULT_NAME`.

Using an external provider keeps sensitive values out of the state file and allows shared access between team members.
