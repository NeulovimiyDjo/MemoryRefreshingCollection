Describe 'PSScriptAnalyzer' {
    It 'Does not return any violations' {
        $settingsPath = Join-Path $PSScriptRoot "PSScriptAnalyzerSettings.psd1"
        $errorMessages = New-Object System.Collections.Generic.List[System.String]
        Invoke-ScriptAnalyzer -Recurse -Settings $settingsPath -Path $PSScriptRoot -ErrorAction SilentlyContinue |
            Select-Object -Property Message, ScriptName, Line, Column |
            ForEach-Object { $errorMessages.Add("$($_.Message) ($($_.ScriptName) $($_.Line) $($_.Column))") }
        $errorMessages | Should -BeNullOrEmpty
    }
}
