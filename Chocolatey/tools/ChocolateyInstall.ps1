# powershell v2 compatibility
$psVer = $PSVersionTable.PSVersion.Major
if ($psver -ge 3) {
    function Get-ChildItemDir { Get-ChildItem -Directory $args }
} else {
    function Get-ChildItemDir { Get-ChildItem $args }
}

$packageName = 'keepass-plugin-winhello'
$keePassDisplayName = 'KeePass Password Safe'
$version = '2.2.0'
$checksum = 'C43DFA64C258DD5E07601261B96DA9C34DF83F1EC338A942B528CB44B89826E1'
$checksumType = 'sha256'
$pluginFileName = 'KeePassWinHelloPlugin.plgx'
$url = "https://github.com/sirAndros/KeePassWinHello/releases/download/v$version/$pluginFileName"

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

    $toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
    $source = "$toolsDir\..\plugin"
    if (!(Test-Path $source)) {
        Write-Verbose "Download plugin files"
        Get-ChocolateyWebFile -PackageName "$packageName" `
                              -Url "$url" `
                              -FileFullPath  "$source\$pluginFileName" `
                              -Checksum "$checksum" `
                              -ChecksumType "$checksumType"
    }
    
    Write-Verbose "Copy plugin files into Plugins dir"
    Copy-Item -Path "$source\*" -Destination $pluginPath -Recurse -Force

    if ( Get-Process -Name "KeePass" -ErrorAction SilentlyContinue ) {
        Write-Warning "$keePassDisplayName is currently running. Plugin will be available at next restart of KeePass process." 
    } else {
        Write-Host "$packageName will be loaded the next time KeePass is started."
        Write-Host "Please note this plugin may require additional configuration. Look for a new entry in KeePass' Tools>Options"
    }
} catch {
    throw $_.Exception
}
