#define AppName "KeePassWinHello Plugin"
#define AppPublisher "SirAndros"
#define AppUrl "https://github.com/sirAndros/KeePassWinHello"

#ifndef AppVersion
#define AppVersion "0.0.0"
#endif
#ifndef SourcePlgx
#define SourcePlgx "..\releases\KeePassWinHelloPlugin.plgx"
#endif
#ifndef OutputDir
#define OutputDir "..\releases"
#endif
#ifndef OutputBaseFilename
#define OutputBaseFilename "KeePassWinHelloPluginSetup-" + AppVersion
#endif

[Setup]
AppId={{8D3B0C26-C886-4A5E-9160-860CA6C6A76C}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
DefaultDirName={code:GetKeePassPluginDir}
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
SetupLogging=yes
UninstallDisplayName={#AppName}
UninstallFilesDir={commonappdata}\KeePassWinHello
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} installer
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}

[Files]
Source: "{#SourcePlgx}"; DestDir: "{app}"; Flags: ignoreversion

[Code]
function HasKeePassExe(DirName: String): Boolean;
begin
  Result := FileExists(AddBackslash(DirName) + 'KeePass.exe');
end;

function TryRegistryPath(RootKey: Integer; SubKeyName: String; var KeePassDir: String): Boolean;
var
  InstallLocation: String;
begin
  Result := False;

  if RegQueryStringValue(RootKey, SubKeyName, 'InstallLocation', InstallLocation) then
  begin
    if HasKeePassExe(InstallLocation) then
    begin
      KeePassDir := InstallLocation;
      Result := True;
    end;
  end;
end;

function TryFixedPath(Path: String; var KeePassDir: String): Boolean;
begin
  Result := False;

  if HasKeePassExe(Path) then
  begin
    KeePassDir := Path;
    Result := True;
  end;
end;

function FindKeePassDir(var KeePassDir: String): Boolean;
begin
  Result :=
    TryRegistryPath(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\KeePassPasswordSafe2_is1', KeePassDir) or
    TryRegistryPath(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\KeePassPasswordSafe2_is1', KeePassDir) or
    TryRegistryPath(HKLM, 'Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\KeePassPasswordSafe2_is1', KeePassDir) or
    TryFixedPath(ExpandConstant('{pf32}\KeePass Password Safe 2'), KeePassDir) or
    TryFixedPath(ExpandConstant('{pf}\KeePass Password Safe 2'), KeePassDir);
end;

function GetKeePassPluginDir(Default: String): String;
var
  KeePassDir: String;
begin
  if FindKeePassDir(KeePassDir) then
  begin
    Result := AddBackslash(KeePassDir) + 'Plugins';
  end
  else
  begin
    Result := ExpandConstant('{pf32}\KeePass Password Safe 2\Plugins');
  end;
end;

function InitializeSetup(): Boolean;
var
  KeePassDir: String;
begin
  Result := FindKeePassDir(KeePassDir);
  if not Result then
  begin
    MsgBox('KeePass 2 was not found. Install KeePass before installing KeePassWinHello.', mbError, MB_OK);
  end;
end;
