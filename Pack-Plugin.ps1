param (
    [string] $ProjectDir = $null,
    [string] $OutputFileNameBase = 'KeePassWinHelloPlugin',
    [string] $OutputDir = $null
)
$keePassExe = 'C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe'

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}
if (!$OutputDir) {
    $OutputDir = "$ProjectDir\out"
}

$sources = "$ProjectDir\src"
$tempDirName = $OutputFileNameBase
$packingSourcesFolder = "$OutputDir\$tempDirName"
if (Test-Path $packingSourcesFolder) {
    Remove-Item $packingSourcesFolder -Force -Recurse
}
New-Item $packingSourcesFolder -Type Directory > $null


$excludedItems = '.*', '*.sln', 'bin', 'obj', '*.user', '*.ps1', '*.plgx', '*.md', $tempDirName, 'Screenshots'
Get-ChildItem $sources | 
    Where-Object   { $i=$_; $res=$true; $excludedItems | ForEach-Object { $i -notlike $_ } | ForEach-Object { $res = $_ -and $res }; $res } |
    ForEach-Object { Copy-Item $_.FullName $packingSourcesFolder -Recurse }

try {
    Push-Location
    Set-Location $OutputDir
    Start-Process $keePassExe -arg '--plgx-create',"`"$packingSourcesFolder`"" -Wait
} finally {
    Pop-Location
}

Remove-Item $packingSourcesFolder -Force -Recurse