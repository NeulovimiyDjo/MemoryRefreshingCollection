#requires -Modules @{ ModuleName = "Pester"; ModuleVersion = "5.3.0" }
$global:ProgressPreference = "SilentlyContinue"

$configuration = [PesterConfiguration]::Default
$configuration.Should.ErrorAction = 'Continue'
$configuration.Run.Exit = $true

$configuration.TestResult.Enabled = $true
$configuration.TestResult.OutputPath = "$PSScriptRoot\Tests\TestsOutput\testResults.xml"

$configuration.CodeCoverage.Enabled = $true
$configuration.CodeCoverage.Path = @("$PSScriptRoot\Classes")
$configuration.CodeCoverage.CoveragePercentTarget = 50
$configuration.CodeCoverage.OutputFormat = "CoverageGutters"
$configuration.CodeCoverage.OutputPath = "$PSScriptRoot\Tests\TestsOutput\Coverage\coverage.xml"

$unitTests = Get-ChildItem -File -Recurse -Path "$PSScriptRoot" -Include "*.Tests.ps1" |
    Select-Object -ExpandProperty FullName
$configuration.Run.Path = @("$PSScriptRoot\PSScriptAnalyzerTests.ps1") + $unitTests

Invoke-Pester -Configuration $configuration
