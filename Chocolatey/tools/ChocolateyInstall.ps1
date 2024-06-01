# powershell v2 compatibility
$psVer = $PSVersionTable.PSVersion.Major
if ($psver -ge 3) {
    function Get-ChildItemDir { Get-ChildItem -Directory $args }
} else {
    function Get-ChildItemDir { Get-ChildItem $args }
}

function GetInstallDirFromRegistry {
    # used code from Get-AppInstallLocation
    param (
        [string] $AppNamePattern
    )

    function strip($path) { if ($path.EndsWith('\')) { return $path -replace '.$' } else { $path } }
    function is_dir( $path ) { $path -and (Get-Item $path -ea 0).PsIsContainer -eq $true }

    Write-Verbose "Trying Uninstall key property 'InstallLocation'"
    $location = $key.InstallLocation
    if (is_dir $location) { return strip $location }

    Write-Verbose "Trying Uninstall key property 'UninstallString'"
    $location = $key.UninstallString
    if ($location) { $location = $location.Replace('"', '') | Split-Path }
    if (is_dir $location) { return strip $location }

    Write-Verbose "Trying Uninstall key property 'DisplayIcon'"
    $location = $key.DisplayIcon
    if ($location) { $location = Split-Path $location }
    if (is_dir $location) { return strip $location }
}

$packageName = 'keepass-plugin-winhello'
$keePassDisplayName = 'KeePass Password Safe'

Write-Verbose "Searching $keePassDisplayName install location..."
$installPath = Get-AppInstallLocation "^$keePassDisplayName" #regex
if (!$installPath) {
    $installPath = GetInstallDirFromRegistry "$keePassDisplayName*" #wildcard pattern for '-like' inside Get-UninstallRegistryKey
}
if (!$installPath) {
    $binRoot = Get-BinRoot
    $portPath = Join-Path $binRoot "keepass"
    Write-Verbose "Searching '$portPath' for portable install..."
    $installPath = Get-ChildItemDir $portPath* -ErrorAction SilentlyContinue
}
if (!$installPath) {
    throw "$keePassDisplayName not found."
}
Write-Verbose "`t...found: $installPath"

Write-Verbose "Searching for plugin directory..."
$pluginPath = (Get-ChildItemDir $installPath\Plugin*).FullName
if ($pluginPath.Count -eq 0) {
    $pluginPath = Join-Path $installPath "Plugins"
    [System.IO.Directory]::CreateDirectory($pluginPath)
}
Write-Verbose "`t...found."

$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$source = "$toolsDir\..\plugin"

Write-Verbose "Copy plugin files into Plugins dir"
Copy-Item -Path "$source\*" -Destination $pluginPath -Recurse -Force

if ( Get-Process -Name "KeePass" -ErrorAction SilentlyContinue ) {
    Write-Warning "$keePassDisplayName is currently running. Plugin will be available at next restart of KeePass process."
} else {
    Write-Host "$packageName will be loaded the next time KeePass is started."
    Write-Host "Please note this plugin may require additional configuration. Look for a new entry in KeePass' Tools>Options"
}
