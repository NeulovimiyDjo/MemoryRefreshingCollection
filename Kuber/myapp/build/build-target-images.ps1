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

$currentBuildTag = "1234"
$cacheTag = "5678abcd12"

function DockerBuild([string]$imageName, [string]$imageTag) {
    $cacheFrom = ""
    if (((docker image ls -q "${imageName}:$cacheTag") | Measure-Object).Count -gt 0) {
        $logger.Debug("Using cache image ${imageName}:$cacheTag")
        $cacheFrom = "--cache-from ${imageName}:$cacheTag"
    }

    $secrets = "--secret type=file,id=nuget,src=.scfg/NuGet.Config --secret type=file,id=npm,src=.scfg/.npmrc"
    $buildCmd = "build --tag ${imageName}:${imageTag} $cacheFrom $secrets --file ./Build/Dockerfile-targets-$imageName --build-arg BUILDKIT_INLINE_CACHE=1 . --progress=$Progress"
    $logger.Debug("Building image ${imageName}:${imageTag}")
    $null = $processRunner.ExecuteProcess("docker", $buildCmd, 0)

    $logger.Debug("Tagging image ${imageName}:${imageTag} to ${imageName}:${currentBuildTag}")
    $null = $processRunner.ExecuteProcess("docker", "tag ${imageName}:${imageTag} ${imageName}:${currentBuildTag}", 0)
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
