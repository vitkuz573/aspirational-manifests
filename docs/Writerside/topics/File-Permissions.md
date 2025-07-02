# File Permissions

Aspir8 writes its state file and generated manifests to disk. Restrict access to these files so only the current user can read them.

On Linux and macOS the recommended permission for `%state-file%` is `600`:

```bash
chmod 600 %state-file%
```

Adjust your `umask` or set permissions manually after generation. Windows users should ensure the file is not shared with other accounts.
