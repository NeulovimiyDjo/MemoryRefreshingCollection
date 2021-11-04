$ErrorActionPreference = "Stop"
$global:ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$hostName = [System.Net.Dns]::GetHostName()
function LogMessage([string]$msg) {
    Write-Host "MESSAGE ON HOST '$($hostName)': $msg"
}

function DeleteIfExists([string]$path) {
    if (Test-Path -Path $path) {
        LogMessage "Deleting '$path'"
        Remove-Item -Force -Path $path
    }
}

function CreateDirIfNotExists([string]$dir) {
    if (-not (Test-Path -Path $dir)) {
        LogMessage "Creating directory '$dir'"
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
}

function Zip([string]$path, [string]$toFile) {
    LogMessage "Zipping '$path' to '$toFile'"
    Compress-Archive -CompressionLevel Fastest -Path $path -DestinationPath $toFile
}

function Unzip([string]$file, [string]$toPath) {
    LogMessage "Unzipping '$file' to '$toPath'"
    Expand-Archive -Force -Path $file -DestinationPath $toPath
}
