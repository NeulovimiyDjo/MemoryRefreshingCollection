#requires -Modules @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.20.0" }

$settingsPath = Join-Path $PSScriptRoot "PSScriptAnalyzerSettings.psd1"
$psFiles = Get-ChildItem -Path $PSScriptRoot -Recurse -Include "*.ps1", "*.psm1"
foreach ($file in $psFiles) {
    $oldFileContent = Get-Content -Path $file.FullName -Raw
    Invoke-ScriptAnalyzer -Settings $settingsPath -Path $file.FullName -Fix | Out-Null
    $newFileContent = Get-Content -Path $file.FullName -Raw
    if ($oldFileContent -ne $newFileContent) {
        Write-Host "Fixed file $($file.FullName)"
    }
}
