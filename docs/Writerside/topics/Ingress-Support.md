# Ingress Support

Aspir8 can optionally generate Kubernetes Ingress resources for services that expose HTTP bindings.
During `generate` or `run` you will be prompted to select the services you wish to expose. For each
service you can provide a host name and optional TLS secret. Selected values are stored in the
project state so subsequent runs reuse them.

When enabled, Aspir8 will also offer to deploy the NGINX ingress controller if it is not found
in the target cluster.

```
aspirate generate --with-ingress
```

```
aspirate run --with-ingress
```

## Cli Options (Optional)

| Option | Environmental Variable | Description |
|-------|-----------------------|-------------|
| --with-ingress | `ASPIRATE_WITH_INGRESS` | Enable ingress configuration non-interactively |
