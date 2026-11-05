## What this changes

<!-- A short description of the change and why it is needed. Link the issue it fixes, if any: "Fixes #123". -->

## How it was tested

<!-- The tests you added or ran (dotnet test tests/Foldicon.Core.Tests -p:Platform=x64 --filter <filter>), and any manual checks. -->

## Checklist

- [ ] Every change of code comes with unit tests that cover the new or changed logic
- [ ] The solution builds with MSBuild without warnings
- [ ] User-facing strings are only in the `Resources.resw` files (and `Foldicon.ContextMenu.rc`), in English and Spanish
- [ ] The code follows the style of the project (one type per file, a comment before each block)
- [ ] The change respects [the engineering guidelines](../docs/engineering-guidelines.md)
- [ ] The documentation in `docs/` and `SECURITY.md` are updated when the behaviour changes
- [ ] `CHANGELOG.md` is updated if users will notice the change
- [ ] The README is updated if this adds or changes something important
- [ ] No secrets, passwords or personal data in the code, tests or commits
