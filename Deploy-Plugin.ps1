param (
    [string] $ProjectDir = $null,
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

if ($ForDebug) {
    Get-ChildItem $ProjectDir -Filter '*.plgx' | ForEach-Object { Join-Path $TargetDir $_.Name | Get-ChildItem -ErrorAction SilentlyContinue | Remove-Item }
    Join-Path $ProjectDir 'bin\Debug' | Get-ChildItem -Filter '*.dll' | Copy-Item -Destination $TargetDir -Force
} else {
    Join-Path $ProjectDir 'bin\Debug' | Get-ChildItem -Filter  '*.dll' | ForEach-Object { Join-Path $TargetDir $_.Name | Get-ChildItem -ErrorAction SilentlyContinue | Remove-Item }
    Get-ChildItem $ProjectDir -Filter '*.plgx' | Copy-Item -Destination $TargetDir -Force
}