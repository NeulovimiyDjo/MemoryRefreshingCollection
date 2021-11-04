#requires -Modules @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.20.0" }

$settingsPath = Join-Path $PSScriptRoot "PSScriptAnalyzerSettings.psd1"
Invoke-ScriptAnalyzer -Recurse -Settings $settingsPath -Path $PSScriptRoot -ErrorAction SilentlyContinue |
    Select-Object -Property RuleName, Message, Severity, ScriptPath, Line, Column
