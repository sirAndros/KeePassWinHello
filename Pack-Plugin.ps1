param (
    [string] $ProjectDir = $null,
    [string] $TempDirName = 'KeePassWinHelloPlugin',
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


if (!(Test-Path $OutputDir)) {
    mkdir $OutputDir > $null
}

$dest = "$OutputDir\$TempDirName"
if (Test-Path $dest) {
    rm $dest -Force -Recurse
}
mkdir $dest > $null

if (!(Test-Path $OutputDir)) {
    mkdir $OutputDir > $null
}

$ExcludedItems = '.*', '*.sln', 'bin', 'obj', '*.user', '*.ps1', '*.plgx', '*.md', $TempDirName, 'Screenshots'
dir $sources | 
    ?{ $i=$_; $res=$true; $ExcludedItems | %{ $i -notlike $_ } | %{ $res = $_ -and $res }; $res } |
    %{ copy $_.FullName $dest -Recurse }

try {
    Push-Location
    cd $OutputDir
    Start-Process $keePassExe -arg '--plgx-create',"`"$dest`"" -Wait
} finally {
    Pop-Location
}


rm $dest -Force -Recurse