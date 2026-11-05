# Contributing to Foldicon

Bug reports, ideas and pull requests are welcome.

## Before you start

- For a bug or an idea, open an issue first, so the change can be discussed before you write it.
- Security problems are reported privately: see [SECURITY.md](SECURITY.md).
- Foldicon is only a folder icon picker. Favorites, recent icons, sync, lists of folders and settings of its own are out
  of scope, and so is any paid dependency or third-party service.

## Setup

1. Install the tools described in [Building from source](README.md#build-from-source).
2. Install [gitleaks](https://github.com/gitleaks/gitleaks/releases) and keep it in your `PATH`.
3. Enable the hooks of the repository, which block any commit where gitleaks finds a secret:

   ```sh
   git config core.hooksPath .githooks
   ```

## Code style

The full guide is in [docs/code-style.md](docs/code-style.md). In short:

- **One type per file**, grouped in folders by scope (`Indexing/`, `Folders/`, `Search/`…).
- **The first line of each file** is a comment that says what it holds.
- **A comment in English before each logical block**, in the third person and present tense ("Stores the…", "Checks
  whether…"), with a blank line between blocks.
- **Indentation of 4 spaces** (2 in XML, JSON and YAML); see `.editorconfig`.
- **User-facing strings only in `src/Foldicon.App/Strings/*/Resources.resw`** (English and Spanish) and, for the
  context menu, in `Foldicon.ContextMenu.rc`. Add every new string to both languages.

## Tests

Every change of code comes with unit tests that cover the new or changed logic. A pull request without them is not
finished. See [docs/code-style.md](docs/code-style.md#tests).

```sh
dotnet test tests/Foldicon.Core.Tests -p:Platform=x64 --filter <filter>
```

## README

The README is the front page of the project, so review it with every important change: a new feature, a new source of
icons, or a change in how Foldicon is installed or built. Update its text, its feature list and its third-party
credits, and take new screenshots when the window changes. `scripts/Render-Readme-Images.ps1` regenerates the images of
`docs/images` from the sources of `docs/images/src`.

## Commits

- Write commit messages in English, as one sentence in the imperative mood that says what the change does, without a
  prefix or a trailing period.
- Keep each commit focused on one change, and stage only the files that belong to it.
- Never bypass the hooks with `--no-verify`.

## License

Foldicon is licensed under the [GNU General Public License, version 3](LICENSE.md) (`GPL-3.0-only`). By contributing,
you agree that your contributions are licensed under the same terms.
