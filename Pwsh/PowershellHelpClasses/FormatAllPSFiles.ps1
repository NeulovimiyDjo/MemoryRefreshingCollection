#requires -Modules @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.20.0" }

$settingsPath = Join-Path $PSScriptRoot "PSScriptAnalyzerSettings.psd1"
$psFiles = Get-ChildItem -Path $PSScriptRoot -Recurse -Include "*.ps1", "*.psm1"
foreach ($file in $psFiles) {
    $oldFileContent = Get-Content -Path $file.FullName -Raw
    $newFileContent = Invoke-Formatter -Settings $settingsPath -ScriptDefinition $oldFileContent
    if ($oldFileContent -ne $newFileContent) {
        Out-File -InputObject $newFileContent -FilePath $file.FullName -Encoding utf8 -Force -NoNewline
        Write-Host "Formatted file $($file.FullName)"
    }
}
