# Configurable State Path

By default, aspirate stores the `%state-file%` in the current working directory. Use the `--state-path` option or set the `ASPIRATE_STATE_PATH` environment variable to change this location.

This allows you to keep the state file outside of your project, for example in a secure directory or when running in CI pipelines. Ensure the directory exists before running aspirate.
