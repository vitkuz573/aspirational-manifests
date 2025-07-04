# Secret Management

Aspirate now includes built-in support for robust secret management, allowing you to easily encrypt sensitive data such as connection strings.
This feature is designed to increase security and minimize vulnerabilities.

Aspir8 which uses AesGcm encryption to encrypt the secret's file using a password.
The user supplies this password during the `generate` and `apply` processes.
It's generated using Pbkdf2 with SHA256, one million iterations by default, and the hash and salt are stored in the secret file.
Secrets protected by this provider are only accessible to users who know the password, and are completely safe to store in a Git repository.
All credentials, including those for private container registries, are stored encrypted in
`aspirate-state.json`.

The iteration count can be tuned for stronger or faster hashing by setting the `ASPIRATE_PBKDF2_ITERATIONS` environment variable or passing `--pbkdf2-iterations` on the command line:

```bash
ASPIRATE_PBKDF2_ITERATIONS=2500000 aspirate generate
```

If unset, the default value of `1_000_000` iterations is used.

The chosen iteration count is saved in the secrets file so subsequent runs use the same value automatically.

## Selecting a Secret Provider

Secrets are stored locally by default. Use the
`--secret-provider` option or the `ASPIRATE_SECRET_PROVIDER` environment
variable to select a different provider.
Available providers:

- `file` - encrypted secrets written to disk (default)
- `env` - read secrets from environment variables
- `base64` - secrets encoded as Base64 in the state file

When supplying the secret password via the `ASPIRATE_SECRET_PASSWORD` environment
variable, Aspirate clears the variable after reading it to avoid leaving the
password in the shell environment.

## Built-in Secret Protection Strategies

The following environment variable names are protected automatically when secrets are saved:

- `ConnectionString*` - any value beginning with this prefix.
- `POSTGRES_PASSWORD`
- `MSSQL_SA_PASSWORD`
- `API_KEY`
- `CLIENT_SECRET`
- `JWT_SECRET`
- `REDIS_PASSWORD`
- `MONGODB_PASSWORD`
- `RABBITMQ_PASSWORD`

## Password Handling

Passwords entered at the CLI are stored using `SecureString` where available. On Windows this provides OS level protection for the in‑memory value. On Linux and macOS the data is still cleared after use but may not be encrypted in memory.
