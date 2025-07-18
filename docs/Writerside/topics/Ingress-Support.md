# Ingress Support

Aspir8 can optionally generate Kubernetes Ingress resources for services that expose HTTP bindings.
When running interactively, Aspir8 automatically prompts you to select the services you wish to expose whenever external bindings are detected. When multiple services are available the prompt includes an
"All Services" group so you can quickly select every option. For each service you can provide one or more host names,
an optional service port, optional TLS secret, and optional ingress annotations. Selected values are stored in the project state so subsequent runs reuse them.

When enabled, Aspir8 will also offer to deploy the NGINX ingress controller if it is not found
in the target cluster.

```bash
aspirate generate
```

```bash
aspirate run
```

Ingress configuration runs automatically when services with external bindings are detected during interactive execution.

When specifying ingress information you may also set the **port number** that should be used by the ingress backend. If omitted, Aspir8 will use the first internal port defined for the service.

## Service port translation

Bindings defined on resources include `port` and `targetPort` values. During
generation these translate directly to a Kubernetes Service's `port` and
`targetPort` respectively. If a binding omits `port`, the value of
`targetPort` is used for both fields.

Ingress forwards traffic to the Service's `port`. When no `port` is specified
for a binding, that Service port defaults to the `targetPort` value. You can use
the optional ingress port number to select which Service port the Ingress rule
should forward traffic to.
