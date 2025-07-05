# Dapr Support

%product% can include `dapr.v0` resources to run Dapr sidecars alongside your components.
When defining a Dapr resource the following fields are required:

- `metadata` – configuration object for the sidecar.
- `metadata.application` – name of the application component the sidecar attaches to.
- `metadata.appId` – Dapr application identifier.
- `metadata.components` – list of components that should be loaded.

If any of these values are omitted, an `InvalidOperationException` is thrown when generating Docker Compose entries.
