# External Providers

Secrets do not have to be stored in the local file provider. Aspir8 allows implementing alternative secret backends.

Choose a provider with the `--secret-provider` option or set `ASPIRATE_SECRET_PROVIDER`.
Built-in providers are:

- `file` - encrypted secrets stored on disk (default)
- `env` - secrets supplied via environment variables
- `base64` - secrets stored as Base64 in the state file

Custom providers can be added by extending the service container.

Using an external provider keeps sensitive values out of the state file and allows shared access between team members.
