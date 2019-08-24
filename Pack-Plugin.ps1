param (
    [string] $ProjectDir = $null,
    [string] $OutputFileNameBase = 'KeePassWinHelloPlugin',
    [string] $OutputDir = $null,
    [string] $Version = $null,
    [string] $ReleaseNotes = $null
)

$versionPattern = '[\d\.]+(?:\-\w+)?'

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}
if (!$OutputDir) {
    $OutputDir = "$ProjectDir\releases"
}
if (!$ReleaseNotes) {
    $ReleaseNotes = Get-Content "$ProjectDir\ReleaseNotes.md" -Raw
}

$sources = "$ProjectDir\src"
$versionFile = "$ProjectDir\keepass.version"
$assInfoPath = "$sources\Properties\AssemblyInfo.cs"

if (!$Version) {
    $Version = (Select-String -Pattern "AssemblyVersion\s*\(\s*['`"]($versionPattern)['`"]\s*\)" -Path $assInfoPath).Matches[0].Groups[1].Value
}
(Get-Content $versionFile) -replace "(?<=\:)$versionPattern", $Version | Set-Content $versionFile


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

$keePassExe = "$ProjectDir\lib\KeePass.exe"
if (!(Test-Path $keePassExe)) {
    $keePassExe = 'C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe'
}

try {
    Push-Location
    Set-Location $OutputDir
    Start-Process $keePassExe -arg '--plgx-create',"`"$packingSourcesFolder`"" -Wait
} finally {
    Pop-Location
}

Remove-Item $packingSourcesFolder -Force -Recurse

$outputFile = "$OutputDir\$OutputFileNameBase.plgx"
$chocoDir = "$ProjectDir\Chocolatey"
$chocoInstallScriptFile = "$chocoDir\tools\ChocolateyInstall.ps1"
$hash = (Get-FileHash $outputFile -Algorithm SHA256).Hash
(Get-Content $chocoInstallScriptFile) `
    -replace "\`$version\s*\=\s*['`"]$versionPattern['`"]", "`$version = '$Version'" `
    -replace "\`$checksum\s*\=\s*['`"][\w\d]+['`"]", "`$checksum = '$hash'" `
    | Set-Content $chocoInstallScriptFile

$chocoVerificationFile = "$chocoDir\tools\VERIFICATION.txt"
(Get-Content $chocoVerificationFile) `
    -replace "checksum\:\s*[\w\d]+", "checksum: $hash" `
    | Set-Content $chocoVerificationFile

& choco pack "`"$chocoDir\keepass-plugin-winhello.nuspec`"" --version $Version --out `"$OutputDir`" ReleaseNotes=`"$ReleaseNotes`"