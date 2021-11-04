using module ".\Logger.psm1"
using module ".\ProcessRunner.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class MSBuildHelper {
    [ValidateNotNullOrEmpty()][string]$MSBuildPath
    [ValidateNotNullOrEmpty()][string]$RepoDir
    [ValidateSet("Release", "Debug")][string]$Configuration

    [ValidateNotNull()][Logger]$Logger

    MSBuildHelper([PSCustomObject]$config, [Logger]$logger) {
        $this.MSBuildPath = $config.MSBuildPath
        $this.RepoDir = $config.RepoDir
        $this.Configuration = $config.Configuration

        $this.Logger = $logger
    }

    [void]BuildSolution([string]$solutionPath) {
        $this.Logger.Debug("Building solution '$solutionPath'")

        $fullSolutionPath = Resolve-Path -Path $solutionPath
        $solutionDir = Split-Path -Path $fullSolutionPath -Parent

        Push-Location $solutionDir
        try {
            $this.ExecMsBuild($fullSolutionPath, @(
                    "/t:Clean",
                    "/m",
                    "/nologo",
                    "/clp:ErrorsOnly",
                    "/p:Configuration=$($this.Configuration)"))

            $this.ExecMsBuild($fullSolutionPath, @(
                    "/t:Restore"
                    "/m"
                    "/nologo"
                    "/clp:ErrorsOnly"
                    "/p:Configuration=$($this.Configuration)"
                    "/p:SolutionDir=$solutionDir\"
                    "/p:RestoreConfigFile=$($this.RepoDir)\NuGet.Config"
                    "/p:RestorePackagesConfig=true"))

            $this.ExecMsBuild($fullSolutionPath, @(
                    "/t:Restore,Build"
                    "/m"
                    "/nologo"
                    "/clp:verbosity=minimal"
                    "/p:Configuration=$($this.Configuration)"
                    "/p:SolutionDir=$solutionDir\"
                    "/p:RestoreConfigFile=$($this.RepoDir)\NuGet.Config"
                    "/flp:Logfile=$($this.RepoDir)\BuildLog.txt;Encoding=UTF-8;Verbosity=minimal"))
        } finally {
            Pop-Location
        }
    }

    [void]ExecMsBuild([string]$solutionPath, [string[]]$arguments) {
        $processRunner = New-Object ProcessRunner $this.Logger
        $null = $processRunner.ExecuteProcess($this.MSBuildPath, (@($solutionPath) + $arguments), 0)
    }
}