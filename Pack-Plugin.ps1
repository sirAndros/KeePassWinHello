param (
    [string] $ProjectDir = $null,
    [string] $TempDirName = 'KeePassWinHelloPlugin'
)

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}

$keePassExe = "$ProjectDir\lib\KeePass.exe"
if (!(Test-Path $keePassExe)) {
    $keePassExe = 'C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe'
}

$dest = "$ProjectDir\$TempDirName"
if (Test-Path $dest) {
    rm $dest -Force -Recurse
}
mkdir $dest > $null

$ExcludedItems = '.*', '*.sln', 'bin', 'obj', '*.user', '*.ps1', '*.plgx', '*.md', $TempDirName, 'Screenshots'
dir $ProjectDir | 
    ?{ $i=$_; $res=$true; $ExcludedItems | %{ $i -notlike $_ } | %{ $res = $_ -and $res }; $res } |
    %{ copy $_.FullName $dest -Recurse }

Start-Process $keePassExe -arg '--plgx-create',"`"$dest`"" -Wait

rm $dest -Force -Recurse