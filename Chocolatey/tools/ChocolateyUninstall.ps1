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
    Write-Warning "$($keePassDisplayName) is running. Please save any opened databases and close $($keePassDisplayName) before attempting to uninstall KeePass plugins."
    throw
}

Write-Verbose "Searching registry for installed KeePass..."
$regPath = Get-ItemProperty -Path @('HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
                                    'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
                                    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*') `
                -ErrorAction:SilentlyContinue `
            | Where-Object { $_.DisplayName -like "$keePassDisplayName*" } `
            | ForEach-Object { $_.InstallLocation }
$installPath = $regPath
if (!$installPath) {
    Write-Verbose "Searching $env:ChocolateyBinRoot for portable install..."
    $binRoot = Get-BinRoot
    $portPath = Join-Path $binRoot "keepass"
    $installPath = Get-ChildItemDir $portPath* -ErrorAction SilentlyContinue
}
if (!$installPath) {
    Write-Verbose "Searching $env:Path for unregistered install..."
    $installFullName = (Get-Command keepass -ErrorAction SilentlyContinue).Path
    if (! $installFullName) {
        $installPath = [io.path]::GetDirectoryName($installFullName)
    }
}
if (!$installPath) {
    Write-Warning "$($keePassDisplayName) not found."
    throw
}
Write-Verbose "`t...found."

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