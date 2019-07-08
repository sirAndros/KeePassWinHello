# powershell v2 compatibility
$psVer = $PSVersionTable.PSVersion.Major
if ($psver -ge 3) {
    function Get-ChildItemDir { Get-ChildItem -Directory $args }
} else {
    function Get-ChildItemDir { Get-ChildItem $args }
}

$packageName = 'keepass-plugin-winhello'
$keePassDisplayName = 'KeePass Password Safe'
$version = '2.1'
$checksum = '4604AA9AC953BD806F82A66F4EBB1F5CDCFC2C0B5666079A6219C6964C1DEAFA'
$checksumType = 'sha256'
$url = "https://github.com/sirAndros/KeePassWinHello/releases/download/v$version/KeePassWinHelloPlugin.plgx"

try {
    Write-Verbose "Searching registry for installed KeePass..."
    $regPath = Get-ItemProperty -Path @('HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
                                        'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
                                        'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*') `
                -ErrorAction:SilentlyContinue `
            | Where-Object { $_.DisplayName -like "$keePassDisplayName*" } `
            | ForEach-Object { $_.InstallLocation }
    
    $installPath = $regPath #todo process multiple installations
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
        Write-Warning "$keePassDisplayName not found."
        throw
    }
    Write-Verbose "`t...found."

    Write-Verbose "Searching for plugin directory..."
    $pluginPath = (Get-ChildItemDir $installPath\Plugin*).FullName
    if ($pluginPath.Count -eq 0) {
        $pluginPath = Join-Path $installPath "Plugins"
        [System.IO.Directory]::CreateDirectory($pluginPath)
    }
    Write-Verbose "`t...found."

    Write-Verbose "Download plugin files into Plugins dir"
    Get-ChocolateyWebFile -PackageName "$packageName" `
                          -Url "$url" `
                          -FileFullPath  "$pluginPath" `
                          -Checksum "$checksum" `
                          -ChecksumType "$checksumType"

    if ( Get-Process -Name "KeePass" -ErrorAction SilentlyContinue ) {
        Write-Warning "$keePassDisplayName is currently running. Plugin will be available at next restart of KeePass process." 
    } else {
        Write-Host "$packageName will be loaded the next time KeePass is started."
        Write-Host "Please note this plugin may require additional configuration. Look for a new entry in KeePass' Tools>Options"
    }
} catch {
    throw $_.Exception
}
