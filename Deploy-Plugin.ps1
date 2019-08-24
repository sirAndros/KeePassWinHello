param (
    [string] $ProjectDir = $null,
    [string] $TargetDir = $null
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

Get-ChildItem $ProjectDir -Filter '*.plgx' | Copy-Item -Destination $TargetDir -Force