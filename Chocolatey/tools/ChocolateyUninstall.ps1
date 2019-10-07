# powershell v2 compatibility
$psVer = $PSVersionTable.PSVersion.Major
if ($psver -ge 3) {
    function Get-ChildItemDir { Get-ChildItem -Directory $args }
}
else {
    function Get-ChildItemDir { Get-ChildItem $args }
}

$keePassDisplayName = 'KeePass Password Safe'
$files = @("KeePassWinHelloPlugin.plgx")

Write-Verbose "Checking KeePass is not running..."
if (Get-Process -Name "KeePass" -ErrorAction SilentlyContinue) {
    throw "$($keePassDisplayName) is running. Please save any opened databases and close $($keePassDisplayName) before attempting to uninstall KeePass plugins."
}

Write-Verbose "Searching $env:ChocolateyBinRoot..."
$installPath = Get-AppInstallLocation "^$keePassDisplayName"

if (!$installPath) {
    Write-Verbose "Searching $env:ChocolateyBinRoot for portable install..."
    $binRoot = Get-BinRoot
    $portPath = Join-Path $binRoot "keepass"
    $installPath = Get-ChildItemDir $portPath* -ErrorAction SilentlyContinue
}
if (!$installPath) {
    throw "$keePassDisplayName not found."
}
Write-Verbose "`t...found: $installPath"

Write-Verbose "Searching for plugin directory..."
$pluginPath = (Get-ChildItemDir $installPath\Plugin*).FullName
if ($pluginPath.Count -eq 0) {
    throw "Plugin directory not found."
}
Write-Verbose "`t...found."

Write-Verbose "Removing plugin files..."
foreach ($file in $files) {
    $installFile = Join-Path $pluginPath $file
    Remove-Item -Path $installFile `
                -Force `
                -ErrorAction SilentlyContinue
}