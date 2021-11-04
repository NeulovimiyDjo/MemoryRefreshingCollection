using module "..\Scripts\Modules\Logger.psm1"
using module "..\Scripts\Modules\ProcessRunner.psm1"
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version,
    [Parameter(Mandatory = $true)][ValidateSet("plain", "auto", IgnoreCase = $false)][string]$Progress
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$logger = New-Object Logger Trace
$processRunner = New-Object ProcessRunner $logger

$cacheTag = "5678abcd12"

function DockerBuild([string]$imageName, [string]$imageTag) {
    $buildCmd = "build --target ${imageName} --tag ${imageName}:${imageTag} --file ./build/Dockerfile-targets . --progress=$Progress"
    $logger.Debug("Building image ${imageName}:${imageTag}")
    $null = $processRunner.ExecuteProcess("docker", $buildCmd, 0)

    if (${imageTag} -ne ${cacheTag}) {
        $logger.Debug("Tagging image ${imageName}:${imageTag} to ${imageName}:${cacheTag}")
        $null = $processRunner.ExecuteProcess("docker", "tag ${imageName}:${imageTag} ${imageName}:${cacheTag}", 0)
    }
}

$repoDir = "$PSScriptRoot/.."
Push-Location $repoDir
try {
    $logger.Info("Building target images...")

    DockerBuild "build" $cacheTag
    DockerBuild "nodebuild" $cacheTag

    DockerBuild "lb" $Version
    DockerBuild "lc" $Version

    DockerBuild "webapp" $Version
    DockerBuild "tools" $Version

    $logger.Info("Prunning dangling images...")
    $null = $processRunner.ExecuteProcess("docker", "image prune --force", 0)
    $logger.Info("Prunning build cache...")
    $null = $processRunner.ExecuteProcess("docker", "builder prune --force", 0)
} finally {
    Pop-Location
}
