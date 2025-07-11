# Using Overlays

Kustomize overlays allow you to keep environment specific patches outside of the base manifests.
An overlay directory typically mirrors the structure created by `aspirate generate` with a `kustomization.yaml` file and subdirectories for each component.

```
overlays/
  dev/
    kustomization.yaml
    webapi/
      deployment-patch.yaml
      .webapi.secrets
    postgrescontainer/
      statefulset-patch.yaml
      .postgrescontainer.secrets
```

`.secrets` files store values referenced by `secretGenerator` entries. If a component directory does not contain one, Aspir8 creates an empty file so kustomize continues without error.

Common patches include adjusting replica counts, setting resource limits or overriding environment variables. Example to scale a deployment:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: webapi
spec:
  replicas: 2
```

Example to set an environment variable:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: webapi
spec:
  template:
    spec:
      containers:
        - name: webapi
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: Development
```
