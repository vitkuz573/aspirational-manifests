# Generating Manifests and Secrets

## Kustomize (Default, Aspir8 managed deployments)

Navigate to your Aspire project's AppHost directory, and run:

```bash
aspirate generate
```
This command (by default) will also build selected projects, and push the containers to the registry (if specified), or the local docker daemon.
Builds can be skipped by passing the `--skip-build` flag.

Your manifests will be in the `%output-dir%` directory by default.

### Using Overlays

To build a specific overlay, supply `--overlay-path` (or `ASPIRATE_OVERLAY_PATH`).
If a component directory in the overlay lacks a `.secrets` file, Aspir8 creates
an empty one so kustomize can run.

```bash
aspirate generate --overlay-path overlays/dev
```

See [Using Overlays](Using-Overlays.md) for details on directory structure and
patch examples.

## Compose

The output format of the manifest can also be changed to compose to generate a lightweight deployment (docker/podman compose).
To generate a docker compose deployment, run:

```bash
aspirate generate --output-format compose
```

Your docker-compose file will be at the path `%output-dir%/docker-compose.yaml` directory by default.

When using the `--output-format compose` flag, you can also build certain dockerfiles using the compose file.
This will skip the build and push in Aspirate.
To do this, include the `--compose-build` flag one or more times.

```bash
aspirate generate --output-format compose --compose-build hamburger --compose-build fries
```

This will build the `hamburger` and `fries` dockerfiles using the compose file.

Compose is what's classed as an "Ejected Deployment" and is not managed by Aspirate when you run it.

## Helm Chart

You also have the option of generating a helm chart by changing the output format to `helm`.
To generate a helm chart, run:

```bash
aspirate generate --output-format helm
```
Helm supports secrets, just like kustomize does, and so you will have to unlock them if you are moving between kustomize and helm.

a Helm chart is what's classed as an "Ejected Deployment" and is not managed by Aspirate when you run it.

## Azure Bicep

Azure Bicep templates can be referenced in your manifest using `azure.bicep.v0` or
`azure.bicep.v1` resources. These entries are preserved during generation so you
can deploy them alongside other components.

## AWS CloudFormation

AWS CloudFormation templates can be referenced in your manifest using `aws.cloudformation.stack.v0` or `aws.cloudformation.template.v0` resources. These entries are also preserved during generation so you can deploy them alongside other components. Unknown properties on these resources are retained when parsing the manifest.

## Manifest extensions

`aspirate` accepts additional fields beyond the official .NET Aspire schema. These extra fields are treated as extensions and are preserved when reading and writing manifest files.

When running interactively, `aspirate` prompts for ingress annotations for each selected external service alongside the host names and TLS secret questions. The supplied values are persisted in the state file so they can be reused on subsequent runs. These annotations apply only to the generated Ingress resource and are **not** read from `manifest.json`.

For details on how binding ports map to Services and how to choose the port used
by Ingress, see [Ingress Support](Ingress-Support.md#service-port-translation).

##Specify components when running with `--non-interactive`
When ran non-interactively, you can specify which components to build with `-c` or `--components`. Example: `-c webApi -c frontend -c sql -c redis`.

## Cli Options (Optional)

| Option                        | Alias | Environmental Variable Counterpart         | Description                                                                                                                                                                                  |
|-------------------------------|-------|--------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| --project-path                | -p    | `ASPIRATE_PROJECT_PATH`                    | The path to the aspire project.                                                                                                                                                              |
| --aspire-manifest             | -m    | `ASPIRATE_ASPIRE_MANIFEST_PATH`            | The aspire manifest file to use                                                                                                                                                              |
| --output-path                 | -o    | `ASPIRATE_OUTPUT_PATH`                     | The path to the output directory. Defaults to `%output-dir%`                                                                                                                                 |
| --overlay-path                | -ol   | `ASPIRATE_OVERLAY_PATH`             | Path to a kustomize overlay directory. If specified, kustomize builds this overlay.
|
| --skip-build                  |       | `ASPIRATE_SKIP_BUILD`                      | Skips build and Push of containers.                                                                                                                                                          |
| --disable-state               |       | `ASPIRATE_DISABLE_STATE`                   | Disable aspirate state management.                                                                                                                                                           |
| --namespace                   |       | `ASPIRATE_NAMESPACE`                       | Generates a Kubernetes Namespace resource, and applies the namespace to all generated resources. Will be used at deployment time.                                                            |
| --skip-final                  | -sf   | `ASPIRATE_SKIP_FINAL_KUSTOMIZE_GENERATION` | Skips The final generation of the kustomize manifest, which is the parent top level file                                                                                                     |
| --container-image-tag         | -ct   | `ASPIRATE_CONTAINER_IMAGE_TAG`             | The Container Image Tag to use as the fall-back value for all containers.                                                                                                                    |
| --container-registry          | -cr   | `ASPIRATE_CONTAINER_REGISTRY`              | The Container Registry to use as the fall-back value for all containers.                                                                                                                     |
| --container-repository-prefix |       | `ASPIRATE_CONTAINER_REPOSITORY_PREFIX`     | The Container Repository Prefix to use as the fall-back value for all containers.                                                                                                            |
| --container-builder           |       | `ASPIRATE_CONTAINER_BUILDER`               | The Container Builder: can be `docker`, `podman` or `nerdctl`. The default is `docker`.                                                                                                                 |
| --prefer-dockerfile           |       | `ASPIRATE_PREFER_DOCKERFILE`               | Instructs to use Dockerfile when available to build project images.                                                                                                                          |
| --container-build-context     | -cbc  | `ASPIRATE_CONTAINER_BUILD_CONTEXT`         | The Container Build Context to use when Dockerfile is used to build projects.                                                                                                                |
| --container-build-arg         | -cba  | `ASPIRATE_CONTAINER_BUILD_ARGS`            | The Container Build Arguments to use for all containers. In `key=value` format. Can include multiple times.                                                                                  |
| --image-pull-policy           |       | `ASPIRATE_IMAGE_PULL_POLICY`               | The image pull policy to use for all containers in generated manifests. Can be `Always`, `Never` or `IfNotPresent`. For your local docker desktop cluster - use `IfNotPresent`               |
| --disable-secrets             |       | `ASPIRATE_DISABLE_SECRETS`                 | Disables secrets management features.                                                                                                                                                        |
| --output-format               |       | `ASPIRATE_OUTPUT_FORMAT`                   | Sets the output manifest format. Defaults to `kustomize`. Can be `kustomize`, `helm` or `compose`.                                                                                           |
| --runtime-identifier          |       | `ASPIRATE_RUNTIME_IDENTIFIER`              | Sets the runtime identifier for project builds. Defaults to `linux-x64`.                                                                                                                     |
| --secret-password             |       | `ASPIRATE_SECRET_PASSWORD`                 | If using secrets, or you have a secret file - Specify the password to decrypt them                                                                                                           |
| --pbkdf2-iterations             |       | `ASPIRATE_PBKDF2_ITERATIONS`                 | Override the PBKDF2 iteration count used for password hashing
| --secret-provider             |       | `ASPIRATE_SECRET_PROVIDER`                 | Secret provider: `file`, `env` or `base64`. Defaults to `file` |
                                    |
| --non-interactive             |       | `ASPIRATE_NON_INTERACTIVE`                 | Disables interactive mode for the command                                                                                                                                                    |
| --private-registry            |       | `ASPIRATE_PRIVATE_REGISTRY`                | Enables usage of a private registry - which will produce image pull secret.                                                                                                                  |
| --private-registry-url        |       | `ASPIRATE_PRIVATE_REGISTRY_URL`            | The url for the private registry                                                                                                                                                             |
| --private-registry-username   |       | `ASPIRATE_PRIVATE_REGISTRY_USERNAME`       | The username for the private registry. This is required if passing `--private-registry`.                                                                                                     |
| --private-registry-password   |       | `ASPIRATE_PRIVATE_REGISTRY_PASSWORD`       | The password for the private registry. This is required if passing `--private-registry`.                                                                                                     |
| --private-registry-email      |       | `ASPIRATE_PRIVATE_REGISTRY_EMAIL`          | The email for the private registry. This is purely optional and will default to `aspirate@aspirate.com`.                                                                                     |
| --include-dashboard           |       | `ASPIRATE_INCLUDE_DASHBOARD`               | Boolean flag to specify if the Aspire dashboard should also be included in deployments.                                                                                                      |
| --compose-build               |       |                                            | Can be included one or more times to set certain dockerfile resource building to be handled by the compose file. This will skip build and push in aspirate.                                  |
| --launch-profile              | -lp   | `ASPIRATE_LAUNCH_PROFILE`                  | The launch profile to use when building the Aspire Manifest.                                                                                                                                 |
| --replace-secrets             |       | `ASPIRATE_REPLACE_SECRETS`                 | The secret state will be completely reinitialised, prompting for a new password. All input values and secrets will be re generated / prompted, and stored in the state.                      |
| --components                  | -c    | `ASPIRATE_COMPONENTS`                      | The components/resources to process. Example: -c webApi -c frontend -c sql -c redis . Components must be written exactly as they appear in manifest.json. Only works when ran non-interactive|
If a private registry is used, a temporary `image-pull-secret.yaml` is written to the OS temp directory during deployment. This file is removed automatically afterwards and should not be committed to source control.
