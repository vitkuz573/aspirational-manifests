# File Permissions

Aspir8 writes its state file and generated manifests to disk. Restrict access to these files so only the current user can read them.
The tool automatically applies secure permissions when creating `%state-file%`. On Unix the mode is set to `600`, while on Windows the ACL grants read and write access only to the current user.

On Linux and macOS the recommended permission for `%state-file%` is `600`:

```bash
chmod 600 %state-file%
```

Adjust your `umask` if needed or set permissions manually when moving the file. Windows users should ensure the file is not shared with other accounts.
