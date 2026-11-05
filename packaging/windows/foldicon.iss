; Inno Setup script of the Windows installer of Foldicon
;
; Packs the self-contained app into Foldicon-Setup-<version>-<platform>.exe, installed in Program Files for the whole computer.
; After copying the files it trusts the public development certificate and registers, for the user who started the
; setup, the package with external location that adds the item to the modern context menu. If that registration fails,
; it falls back to a classic item, which shows under "Show more options". Build it with scripts\Build-Installer.ps1,
; which prepares the payload folder and passes /DAppVersion=<version>, /DPayload=<folder> and /DPlatform=<x64 or arm64>.

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif

#ifndef Payload
  #define Payload "..\..\obj\installer\x64"
#endif

#ifndef Platform
  #define Platform "x64"
#endif

; The x64 installer also runs on ARM64 through emulation, as before; the ARM64 one only runs there
#if Platform == "arm64"
  #define Architectures "arm64"
#else
  #define Architectures "x64compatible"
#endif

#define AppName "Foldicon"
#define AppPublisher "EasyXploit"
#define AppExe "Foldicon.exe"
#define PowerShell "{sys}\WindowsPowerShell\v1.0\powershell.exe"
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

; Runs only on 64-bit Windows 11, the first version with Mica
ArchitecturesAllowed={#Architectures}
ArchitecturesInstallIn64BitMode={#Architectures}
MinVersion=10.0.22000

SetupIconFile=..\..\src\Foldicon.App\Assets\Foldicon.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}

; Writes the installer in dist, which is not versioned
OutputDir=..\..\dist
OutputBaseFilename=Foldicon-Setup-{#AppVersion}-{#Platform}
Compression=lzma2/ultra
SolidCompression=yes
WizardStyle=modern

; Follows the system display language, without asking or keeping the one from a previous install
LanguageDetectionMethod=uilanguage
ShowLanguageDialog=no
UsePreviousLanguage=no

; The only per-user data is the cache and the settings of whoever uninstalls, removed on purpose
UsedUserAreasWarning=no

; Keeps a log in the temporary folder, to see why the context menu registration failed
SetupLogging=yes

[Languages]
; English goes first: it is used on any system that is not in Spanish
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
spanish.ChangeIcon=Cambiar icono
english.ChangeIcon=Change icon
spanish.FinishedShell=Haz clic derecho en una carpeta y elige «Cambiar icono». Si aún no aparece, cierra la sesión y vuelve a entrar.
english.FinishedShell=Right-click a folder and choose "Change icon". If it does not show yet, sign out and in again.
spanish.FinishedClassic=Windows no ha aceptado el menú moderno. Haz clic derecho en una carpeta, elige «Mostrar más opciones» y luego «Cambiar icono».
english.FinishedClassic=Windows did not accept the modern menu. Right-click a folder, choose "Show more options" and then "Change icon".

[Files]
Source: "{#Payload}\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#Payload}\shell\*"; DestDir: "{app}\shell"; Flags: ignoreversion

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\{#AppName}"

[UninstallRun]
Filename: "{#PowerShell}"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\shell\Unregister-Shell.ps1"""; Flags: runhidden; RunOnceId: "UnregisterShell"

[Code]
var
  ClassicFallback: Boolean;

{ Trusts the certificate in the machine's Trusted People store, and returns whether it worked }
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
  Result := ExecAsOriginalUser(ExpandConstant('{#PowerShell}'),
    ExpandConstant('-NoProfile -ExecutionPolicy Bypass -File "{app}\shell\Register-Shell.ps1" -InstallFolder "{app}"'),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
  Log(Format('Registration of the context menu package finished with code %d', [ResultCode]));
end;

{ Adds the classic folder context menu item, for every user on the computer }
procedure AddClassicVerb;
begin
  RegWriteStringValue(HKLM, '{#ClassicVerbKey}', '', CustomMessage('ChangeIcon'));
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

{ Says on the last page where the menu item is }
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
