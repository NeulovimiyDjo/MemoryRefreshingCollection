using module "..\Scripts\Modules\Logger.psm1"
using module "..\Scripts\Modules\ProcessRunner.psm1"
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)][ValidateSet("plain", "auto", IgnoreCase = $false)][string]$Progress
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$logger = New-Object Logger Trace
$processRunner = New-Object ProcessRunner $logger

function DockerBuild([string]$imageName, [string]$imageTag) {
    if (((docker image ls -q "${imageName}:${imageTag}") | Measure-Object).Count -eq 0) {
        $logger.Debug("Building image ${imageName}:${imageTag}")
        $null = $processRunner.ExecuteProcess("docker", "build --target ${imageName} --tag ${imageName}:${imageTag} --file ./build/Dockerfile-bases . --progress=$Progress", 0)
    } else {
        $logger.Debug("Skipping image ${imageName}:${imageTag} already exists")
    }
}

$repoDir = "$PSScriptRoot/.."
Push-Location $repoDir
try {
    $baseTag = "1234abcd56"

    $logger.Info("Building base images...")

    DockerBuild "lbbase" $baseTag
    DockerBuild "lcbase" $baseTag

    DockerBuild "buildbase" $baseTag
    DockerBuild "nodebase" $baseTag

    DockerBuild "aspnetbase" $baseTag
    DockerBuild "toolsbase" $baseTag

    $logger.Info("Prunning dangling images...")
    $null = $processRunner.ExecuteProcess("docker", "image prune --force", 0)
    $logger.Info("Prunning build cache...")
    $null = $processRunner.ExecuteProcess("docker", "builder prune --force", 0)
} finally {
    Pop-Location
}
