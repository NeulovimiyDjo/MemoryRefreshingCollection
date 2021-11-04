[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet("Release", "Debug")][string]$Configuration = "Release",
    [bool]$RunIntegrationTests = $true
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function exec([string]$_cmd) {
    $datetime = [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host -ForegroundColor DarkGray "$datetime >>> exec $_cmd $args"
    & $_cmd @args
    if ($LASTEXITCODE -ne 0) {
        throw "exec '$_cmd $args' failed with exit code $LASTEXITCODE"
    }
}

$repoDir = "$PSScriptRoot"
Push-Location $repoDir
try {
    Write-Host "Running unit tests"
    exec dotnet test -c $Configuration --no-build --nologo "$repoDir\UnitTests\UnitTests.csproj"

    if ($RunIntegrationTests) {
        Write-Host "Running integration tests"
        exec dotnet test -c $Configuration --nologo "$repoDir\IntegrationTests\IntegrationTests.csproj"
    }
} finally {
    Pop-Location
}
