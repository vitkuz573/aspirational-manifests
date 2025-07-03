# External Providers

Secrets do not have to be stored in the local file provider. Aspir8 allows implementing alternative secret backends.

Choose a provider with the `--secret-provider` option or set `ASPIRATE_SECRET_PROVIDER`. The default provider is `file`. Custom providers can be added by extending the service container.

Using an external provider keeps sensitive values out of the state file and allows shared access between team members.
