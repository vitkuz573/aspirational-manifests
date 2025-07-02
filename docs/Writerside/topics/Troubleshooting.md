# Troubleshooting

**Cannot decrypt secrets**
- Confirm the correct password and verify `ASPIRATE_STATE_PATH` if using a custom location.

**Missing Kubernetes context**
- Check your kubeconfig and ensure the desired context exists before running `apply`.

**State file not found**
- Run `generate` to create secrets or verify the state path used by aspirate.
