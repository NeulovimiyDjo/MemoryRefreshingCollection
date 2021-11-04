using module "..\Scripts\Modules\Logger.psm1"
using module "..\Scripts\Modules\ProcessRunner.psm1"
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$RegistryAddress,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$logger = New-Object Logger Trace
$processRunner = New-Object ProcessRunner $logger

function DockerPush([string]$imageName, [string]$imageTag, [string]$registry) {
    $logger.Debug("Tagging image ${imageName}:${imageTag} to $registry/${imageName}:${imageTag}")
    $null = $processRunner.ExecuteProcess("docker", "tag ${imageName}:${imageTag} ${registry}/${imageName}:${imageTag}", 0)
    try {
        $logger.Debug("Pushing image ${registry}/${imageName}:${imageTag}")
        $null = $processRunner.ExecuteProcess("docker", "push ${registry}/${imageName}:${imageTag}", 0)
    } finally {
        $logger.Debug("Removing image ${registry}/${imageName}:${imageTag}")
        $null = $processRunner.ExecuteProcess("docker", "rmi --force ${registry}/${imageName}:${imageTag}", 0)
    }
}

$logger.Info("Pushing target images...")

DockerPush "lb" $Version $RegistryAddress
DockerPush "lc" $Version $RegistryAddress

DockerPush "webapp" $Version $RegistryAddress
DockerPush "tools" $Version $RegistryAddress
