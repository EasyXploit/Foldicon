# Microsoft Store

What the submission of Foldicon needs. The window of the app and its package already meet the rest.

## Build

1. Reserve the name in Partner Center and copy *Package/Identity/Name* and *Package/Identity/Publisher* from *Product
   identity*.
2. Build the package with both platforms:

   ```powershell
   .\scripts\Build-StorePackage.ps1 -IdentityName <name> -Publisher <publisher>
   ```

3. Upload the `.msixupload` of `dist\store`. The Store signs it.
4. Run the Windows App Certification Kit on the package first.

The Store takes the MSIX. The Inno Setup installer stays for GitHub Releases, and it should not be installed on the same
PC, since each one would add its own context menu item.

## Submission notes

- **Restricted capability `runFullTrust`:** "Foldicon is a packaged WinUI 3 desktop app. It needs full trust to write
  `desktop.ini` in the folders the user chooses, through the shell function that customizes a folder, and to register
  its command in the context menu of File Explorer through a COM surrogate."
- **Notes for certification:** "Open the app from Start and choose a folder, or right-click a folder in File Explorer
  and choose Change icon. On first use, sign out and in again if the item does not show."
- **Privacy policy URL:** the address of `PRIVACY.md` in the public repository.
- **Age rating:** answer no to every question of the questionnaire. The app has no user content, purchases, location or
  communication.
- **Category:** Utilities & tools.
- **Languages:** English and Spanish, the two of the package.
- **Minimum system:** Windows 11, x64 and ARM64.

## Listing

**Short description (English):** Give every folder a look of its own.

**Description (English):**

> Right-click a folder, choose Change icon, find the one you like, done.
>
> Foldicon puts the icons you already have in Windows to work and adds 224 icons of the Fluent family. Search in English
> or Spanish, browse by library, and check each icon at the sizes File Explorer uses before you apply it.
>
> Foldicon is free and open source, and it makes no network connections. If a sync client changes a folder that has a
> Fluent icon, Foldicon repairs it when it starts. You can turn this off in the app.

**Short description (Spanish):** Dale a cada carpeta un aspecto propio.

**Description (Spanish):**

> Haz clic derecho en una carpeta, elige Cambiar icono, encuentra el que te guste y listo.
>
> Foldicon aprovecha los iconos que ya tienes en Windows y añade 224 iconos de la familia Fluent. Busca en inglés o en
> español, explora por biblioteca y comprueba cada icono en los tamaños que usa el Explorador antes de aplicarlo.
>
> Foldicon es gratuito y de código abierto, y no se conecta a la red. Si un cliente de sincronización cambia una carpeta
> con un icono Fluent, Foldicon la repara al iniciarse. Puedes desactivarlo en la app.

**Screenshots:** the ones of `docs/images/src`, at 1366 × 768 or larger.

Do not use the names Windows or Microsoft in a way that suggests the app comes from them.
