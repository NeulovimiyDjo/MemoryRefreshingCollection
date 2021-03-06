#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0
Import-Module "$PSScriptRoot/common.psm1"

$repoRoot = "$PSScriptRoot/../.."
Push-Location $repoRoot
try {
    exec dotnet build-server shutdown
    New-Item -Path "./artifacts/nuget" -ItemType Directory -Force | Out-Null
    Copy-Item -Path "./NuGet.Config" -Destination "./artifacts/nuget/NuGet.Config" -Force
    exec dotnet nuget enable source "GeneratedDebugPackages" --configfile "./artifacts/nuget/NuGet.Config"
    #exec dotnet nuget locals --clear global-packages

    exec dotnet build SomeProject.sln -c Debug
    exec dotnet pack --no-build SomeProject.sln -c Debug

    exec dotnet build SomeProject.sln -c Release --configfile "./artifacts/nuget/NuGet.Config" "-p:UsePackagesInSampleApplications=true" "-p:CI_BUILD=true"
    exec dotnet pack --no-build SomeProject.sln -c Release
} finally {
    Pop-Location
}
