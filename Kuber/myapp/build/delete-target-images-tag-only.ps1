[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

Write-Host "Deleting target images (tag $Version only)..."

docker rmi --force "lb:$Version"
docker rmi --force "lc:$Version"

docker rmi --force "webapp:$Version"
docker rmi --force "tools:$Version"
