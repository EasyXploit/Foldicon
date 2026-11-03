# Security policy

Foldicon is in early development and has no published releases yet, so only the `main` branch is supported.

Please do not open public issues for security problems. Report them privately through the *Report a vulnerability*
button of the *Security* tab of the repository, with the steps to reproduce the problem and the version of Windows.

## What Foldicon touches

- The `desktop.ini` of the folder you choose, and the read-only and system attributes of that folder.
- Its own cache in `%LOCALAPPDATA%\Foldicon`, which only holds the index of the system icons.
- The system libraries listed in the README, which it maps as image resources only (`LOAD_LIBRARY_AS_IMAGE_RESOURCE`),
  so none of their code runs.

It makes no network connections.
