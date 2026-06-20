[CmdletBinding()]
param (
    [string] $ProjectDir = $null,
    [string] $OutputDir = $null,
    [string] $Version = $null,
    [string] $InstallerUrl = $null,
    [string] $InstallerPath = $null,
    [string] $InnoSetupCompiler = $null,
    [string] $ReleaseDate = $null,
    [switch] $SkipPluginPack,
    [switch] $SkipInstallerBuild,
    [switch] $SkipManifest
)

$ErrorActionPreference = 'Stop'
$versionPattern = '[\d\.]+(?:\-\w+)?'
$packageIdentifier = 'SirAndros.KeePassWinHello'
$manifestVersion = '1.12.0'

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}
if (!$OutputDir) {
    $OutputDir = Join-Path $ProjectDir 'releases'
}
if (!$ReleaseDate) {
    $ReleaseDate = Get-Date -Format 'yyyy-MM-dd'
}
if (!$Version) {
    $assInfoPath = Join-Path $ProjectDir 'src\Properties\AssemblyInfo.cs'
    $Version = (Select-String -Pattern "AssemblyVersion\s*\(\s*['`"]($versionPattern)['`"]\s*\)" -Path $assInfoPath).Matches[0].Groups[1].Value
}

function Get-InnoSetupCompiler {
    param (
        [string] $RequestedPath
    )

    if ($RequestedPath) {
        if (Test-Path $RequestedPath) {
            return $RequestedPath
        }
        throw "Inno Setup compiler was not found at '$RequestedPath'."
    }

    $command = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Path
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    throw "Inno Setup 6 was not found. Install it or pass -InnoSetupCompiler with the path to ISCC.exe."
}

function Write-Utf8NoBom {
    param (
        [string] $Path,
        [string] $Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$plgxPath = Join-Path $OutputDir 'KeePassWinHelloPlugin.plgx'
if (!$SkipPluginPack) {
    & "$ProjectDir\Pack-Plugin.ps1" -ProjectDir $ProjectDir -OutputDir $OutputDir -Version $Version -SkipChoco
}
if (!(Test-Path $plgxPath)) {
    throw "Plugin package was not found at '$plgxPath'. Build it first or run without -SkipPluginPack."
}

if (!$InstallerPath) {
    $InstallerPath = Join-Path $OutputDir "KeePassWinHelloPluginSetup-$Version.exe"
}

if (!$SkipInstallerBuild) {
    $compiler = Get-InnoSetupCompiler $InnoSetupCompiler
    $installerScript = Join-Path $ProjectDir 'WinGet\KeePassWinHelloPlugin.iss'
    $installerOutputDir = Split-Path -Parent $InstallerPath
    $installerOutputBaseName = [System.IO.Path]::GetFileNameWithoutExtension($InstallerPath)

    New-Item -ItemType Directory -Path $installerOutputDir -Force | Out-Null
    & $compiler `
        "/DAppVersion=$Version" `
        "/DSourcePlgx=$plgxPath" `
        "/DOutputDir=$installerOutputDir" `
        "/DOutputBaseFilename=$installerOutputBaseName" `
        $installerScript
}
else {
    Write-Host "Skipped installer build; using '$InstallerPath'."
}

if (!(Test-Path $InstallerPath)) {
    throw "WinGet installer was not found at '$InstallerPath'. Build it first or pass -InstallerPath."
}

if (!$SkipManifest) {
    if (!$InstallerUrl) {
        throw "Pass -InstallerUrl with the final versioned GitHub release URL for '$([System.IO.Path]::GetFileName($InstallerPath))'."
    }

    $hash = (Get-FileHash $InstallerPath -Algorithm SHA256).Hash
    $manifestDir = Join-Path $OutputDir "winget\manifests\s\SirAndros\KeePassWinHello\$Version"
    New-Item -ItemType Directory -Path $manifestDir -Force | Out-Null

    $versionManifest = @"
PackageIdentifier: $packageIdentifier
PackageVersion: $Version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: $manifestVersion
"@

    $localeManifest = @"
PackageIdentifier: $packageIdentifier
PackageVersion: $Version
PackageLocale: en-US
Publisher: SirAndros
PublisherUrl: https://github.com/sirAndros
PackageName: KeePassWinHello
PackageUrl: https://github.com/sirAndros/KeePassWinHello
License: MIT
LicenseUrl: https://github.com/sirAndros/KeePassWinHello/blob/master/LICENSE
ShortDescription: KeePass 2 plugin to unlock databases with Windows Hello.
Description: KeePassWinHello stores an encrypted database key after normal unlock and uses Windows Hello for later unlocks.
Moniker: keepasswinhello
Tags:
- keepass
- keepass-plugin
- password-manager
- windows-hello
- biometrics
ManifestType: defaultLocale
ManifestVersion: $manifestVersion
"@

    $installerManifest = @"
PackageIdentifier: $packageIdentifier
PackageVersion: $Version
InstallerLocale: en-US
InstallerType: inno
Scope: machine
InstallModes:
- silent
- silentWithProgress
InstallerSwitches:
  Silent: /VERYSILENT /NORESTART
  SilentWithProgress: /SILENT /NORESTART
UpgradeBehavior: install
Dependencies:
  PackageDependencies:
  - PackageIdentifier: DominikReichl.KeePass
Installers:
- Architecture: neutral
  InstallerUrl: $InstallerUrl
  InstallerSha256: $hash
  ReleaseDate: $ReleaseDate
  AppsAndFeaturesEntries:
  - DisplayName: KeePassWinHello Plugin
    Publisher: SirAndros
    DisplayVersion: $Version
ManifestType: installer
ManifestVersion: $manifestVersion
"@

    Write-Utf8NoBom (Join-Path $manifestDir "$packageIdentifier.yaml") $versionManifest
    Write-Utf8NoBom (Join-Path $manifestDir "$packageIdentifier.locale.en-US.yaml") $localeManifest
    Write-Utf8NoBom (Join-Path $manifestDir "$packageIdentifier.installer.yaml") $installerManifest

    Write-Host "Created WinGet manifests in '$manifestDir'."
}

Write-Host "Created WinGet installer package at '$InstallerPath'."
