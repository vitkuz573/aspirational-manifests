# Applying Manifests to a Cluster

To apply the manifests to your cluster, run:

If an overlay path was provided during `generate`, Aspirate uses that directory when applying manifests.

```bash
aspirate apply
```

You will first be presented with all the context names (unless you have passed one in as a cli option) in your kubeconfig file, and will be asked to select one.
This will be used for deployment

If you have a secret file, you will be prompted to enter the password to decrypt it.

Any secrets are written to temporary files within the operating system's temp directory. These files are created with restricted permissions and are automatically removed after deployment.
When deploying with a private registry, Aspirate also generates an `image-pull-secret.yaml`
file in the temp directory. This manifest contains your registry credentials and is deleted
once the deployment finishes. Avoid committing this temporary file to source control.

## Cli Options (Optional)

| Option            | Alias | Environmental Variable Counterpart | Description                                                                                         |
|-------------------|-------|------------------------------------|-----------------------------------------------------------------------------------------------------|
| --input-path      | -i    | `ASPIRATE_INPUT_PATH`              | The path for the kustomize manifests directory. Defaults to `%output-dir%`                          |
| --kube-context    | -k    | `ASPIRATE_KUBERNETES_CONTEXT`      | The name of the kubernetes context within your kubeconfig to apply / deploy manifests to.           |
| --secret-password |       | `ASPIRATE_SECRET_PASSWORD`         | If using secrets, or you have a secret file - Specify the password to decrypt them                  |
| --pbkdf2-iterations |       | `ASPIRATE_PBKDF2_ITERATIONS`      | Override the PBKDF2 iteration count used for password hashing |
| --non-interactive |       | `ASPIRATE_NON_INTERACTIVE`         | Disables interactive mode for the command                                                           |
| --disable-secrets |       | `ASPIRATE_DISABLE_SECRETS`         | Disables secrets management features.                                                               |
| --rolling-restart | -r    | `ASPIRATE_ROLLING_RESTART`         | Perform a rollout restart of deployments directly after applying the manifests. Defaults to `false` |