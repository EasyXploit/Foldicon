# Security policy

Only the latest release is supported.

Please do not open public issues for security problems. Report them privately through the *Report a vulnerability*
button of the *Security* tab of the repository, with the steps to reproduce the problem and the version of Windows.

## What Foldicon touches

- The `desktop.ini` of the folder you choose, and the read-only and system attributes of that folder.
- A hidden copy of the icon, `.foldicon-<id>.ico`, inside that folder when you choose a Fluent icon.
- Other folders that already hold such a copy: every start of the app, unless you turn it off, and *Repair all icons*
  look for them, and put
  back their `desktop.ini`, its marks and the mark of the folder when a sync client lost them. Folders without a copy
  are never written to. See [the architecture](docs/architecture.md#repair-of-synced-folders).
- Trusted People of the machine, where the setup adds the public certificate of its context menu package and the
  uninstaller removes it, and `HKLM\Software\Classes\Directory\shell\Foldicon`, when the setup has to add the classic
  menu item.
- Its own folder in `%LOCALAPPDATA%\Foldicon`, which only holds the index of the icons and the settings of the app.
- The system libraries listed in the README and its own `fluenticons.dll`, which it maps as image resources only
  (`LOAD_LIBRARY_AS_IMAGE_RESOURCE`), so none of their code runs.

It makes no network connections.
