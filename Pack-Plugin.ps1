param (
    [string] $ProjectDir = $null,
    [string] $TempDirName = 'KeePassWinHelloPlugin'
)
$keePassExe = 'C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe'

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}

if (Test-Path $TempDirName) {
    rm $TempDirName -Force -Recurse
}
mkdir $TempDirName > $null

$ExcludedItems = '.*', '*.sln', 'bin', 'obj', '*.user', '*.ps1', '*.plgx', $TempDirName
dir $ProjectDir | 
    ?{ $i=$_; $res=$true; $ExcludedItems | %{ $i -notlike $_ } | %{ $res = $_ -and $res }; $res } |
    %{ copy $_.FullName $TempDirName -Recurse }

Start-Process $keePassExe -arg '--plgx-create',"`"$ProjectDir\$TempDirName`"" -Wait

rm $TempDirName -Force -Recurse