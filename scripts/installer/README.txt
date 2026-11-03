Foldicon - local installer
==========================

Install
  Double-click Install.cmd. Windows asks once for administrator rights to trust the certificate of Foldicon
  (Foldicon.cer, a development certificate of Sencimo Labs); the app itself installs for your user.
  Then right-click a folder and choose "Change folder icon...". If the item does not show yet, sign out and in again.

Uninstall
  Double-click Uninstall.cmd. To also stop trusting the certificate, run in PowerShell:
  .\Uninstall.ps1 -RemoveCertificate
  The icons already applied stay on the folders; "Restore default" in Foldicon removes them.

Requirements
  Windows 10 2004 (build 19041) or later. Nothing else: the package carries .NET, and the Windows App Runtime is in
  the Dependencies folder.


Foldicon - instalador local
===========================

Instalar
  Haz doble clic en Install.cmd. Windows pide una vez permisos de administrador para confiar en el certificado de
  Foldicon (Foldicon.cer, un certificado de desarrollo de Sencimo Labs); la app se instala para tu usuario.
  Después, haz clic derecho en una carpeta y elige "Cambiar icono de carpeta...". Si aún no aparece, cierra la sesión
  y vuelve a entrar.

Desinstalar
  Haz doble clic en Uninstall.cmd. Para dejar de confiar también en el certificado, ejecuta en PowerShell:
  .\Uninstall.ps1 -RemoveCertificate
  Los iconos ya aplicados se quedan en las carpetas; "Restaurar predeterminado" en Foldicon los quita.

Requisitos
  Windows 10 2004 (compilación 19041) o posterior. Nada más: el paquete lleva .NET dentro, y Windows App Runtime
  está en la carpeta Dependencies.
