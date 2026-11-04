; Inno Setup script of the Windows installer of Foldicon
;
; Packs the self-contained app into Foldicon-Setup-<version>.exe, installed in Program Files for the whole computer.
; After copying the files it trusts the public development certificate and registers, for the user who started the
; setup, the package with external location that adds the item to the modern context menu. If that registration fails,
; it falls back to a classic item, which shows under "Show more options". Build it with scripts\Build-Installer.ps1,
; which prepares the folder of the payload and passes /DAppVersion=<version> and /DPayload=<folder>.

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif

#ifndef Payload
  #define Payload "..\..\obj\installer\x64"
#endif

#define AppName "Foldicon"
#define AppPublisher "Sencimo Labs"
#define AppExe "Foldicon.exe"
#define ClassicVerbKey "Software\Classes\Directory\shell\Foldicon"

[Setup]
; Identifies the app across versions so updates replace the previous install; never change it
AppId={{148E857E-F6BD-44CD-A477-02634D0B1504}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppCopyright=Copyright (C) 2026 {#AppPublisher}
VersionInfoVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoCompany={#AppPublisher}

; Needs administrator rights to write in Program Files and to trust the certificate
PrivilegesRequired=admin
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
UsePreviousAppDir=yes

; Runs only on 64-bit Windows from the first version with packages with external location
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041

SetupIconFile=..\..\src\Foldicon.App\Assets\Foldicon.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}

; Writes the installer in dist, which is not versioned
OutputDir=..\..\dist
OutputBaseFilename=Foldicon-Setup-{#AppVersion}
Compression=lzma2/ultra
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
spanish.ChangeFolderIcon=Cambiar icono de carpeta...
english.ChangeFolderIcon=Change folder icon...
spanish.FinishedShell=Haz clic derecho en una carpeta y elige «Cambiar icono de carpeta...». Si aún no aparece, cierra la sesión y vuelve a entrar.
english.FinishedShell=Right-click a folder and choose "Change folder icon...". If it does not show yet, sign out and in again.
spanish.FinishedClassic=Windows no ha aceptado el menú moderno. Haz clic derecho en una carpeta, elige «Mostrar más opciones» y luego «Cambiar icono de carpeta...».
english.FinishedClassic=Windows did not accept the modern menu. Right-click a folder, choose "Show more options" and then "Change folder icon...".

[Files]
Source: "{#Payload}\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#Payload}\shell\*"; DestDir: "{app}\shell"; Flags: ignoreversion

[UninstallRun]
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\shell\Unregister-Shell.ps1"""; Flags: runhidden; RunOnceId: "UnregisterShell"

[Code]
var
  ClassicFallback: Boolean;

{ Trusts the certificate in Trusted People of the machine, and returns whether it worked }
function TrustCertificate: Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec(ExpandConstant('{sys}\certutil.exe'),
    ExpandConstant('-addstore -f TrustedPeople "{app}\shell\Foldicon.cer"'), '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
    and (ResultCode = 0);
end;

{ Registers the package as the user who started the setup, who may not be the administrator, and returns whether it worked }
function RegisterShellPackage: Boolean;
var
  ResultCode: Integer;
begin
  Result := ExecAsOriginalUser('powershell.exe',
    ExpandConstant('-NoProfile -ExecutionPolicy Bypass -File "{app}\shell\Register-Shell.ps1" -InstallFolder "{app}"'),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

{ Adds the classic item of the context menu of folders, for every user of the computer }
procedure AddClassicVerb;
begin
  RegWriteStringValue(HKLM, '{#ClassicVerbKey}', '', CustomMessage('ChangeFolderIcon'));
  RegWriteStringValue(HKLM, '{#ClassicVerbKey}', 'Icon', ExpandConstant('{app}\{#AppExe}'));
  RegWriteStringValue(HKLM, '{#ClassicVerbKey}\command', '', ExpandConstant('"{app}\{#AppExe}" --pick "%1"'));
end;

{ Registers the context menu once the files are copied }
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    ClassicFallback := not (TrustCertificate and RegisterShellPackage);
    if ClassicFallback then
      AddClassicVerb
    else
      RegDeleteKeyIncludingSubkeys(HKLM, '{#ClassicVerbKey}');
  end;
end;

{ Says on the last page where the item of the menu is }
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
    if ClassicFallback then
      WizardForm.FinishedLabel.Caption := CustomMessage('FinishedClassic')
    else
      WizardForm.FinishedLabel.Caption := CustomMessage('FinishedShell');
end;

{ Removes the classic item, if the setup added it }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    RegDeleteKeyIncludingSubkeys(HKLM, '{#ClassicVerbKey}');
end;
