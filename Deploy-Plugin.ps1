[CmdletBinding()]
param (
    [string] $ProjectDir = $null,
    [string] $SourceDir = $null,
    [string] $TargetDir = $null,
    [switch] $ForDebug
)

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}

if (!$TargetDir) {
    $keePassDir = Split-Path -Parent -Path (Get-Item "$ProjectDir\lib\KeePass.exe").Target
    $TargetDir = Join-Path $keePassDir 'Plugins'
}

Write-Output $SourceDir

if (!$SourceDir) {
    $binOutDir = 'src\bin\Debug\net48'
    $SourceDir = Join-Path $ProjectDir $binOutDir
}

$packageOutDir = 'releases'

if ($ForDebug) {
    Join-Path $ProjectDir $packageOutDir | Get-ChildItem -Filter '*.plgx' | ForEach-Object { Join-Path $TargetDir $_.Name | Get-ChildItem -ErrorAction SilentlyContinue | Remove-Item }
    Get-ChildItem $SourceDir -Filter '*.dll' | Copy-Item -Destination $TargetDir -Force
} else {
    Get-ChildItem $SourceDir -Filter '*.dll' | ForEach-Object { Join-Path $TargetDir $_.Name | Get-ChildItem -ErrorAction SilentlyContinue | Remove-Item }
    Join-Path $ProjectDir $packageOutDir | Get-ChildItem -Filter '*.plgx' | Copy-Item -Destination $TargetDir -Force
}