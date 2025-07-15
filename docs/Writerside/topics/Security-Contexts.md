# Security Contexts

%product% supports configuring basic Kubernetes security contexts for generated pods.
When running `aspirate generate` interactively you will be prompted to provide values such as `runAsUser`, `runAsGroup` and `fsGroup` for each container resource.  
The entered values are stored in the project state file so future runs can reuse them without re-entering the information.

If configured, the resulting manifests include the provided `securityContext` on the Pod specification and containers.
