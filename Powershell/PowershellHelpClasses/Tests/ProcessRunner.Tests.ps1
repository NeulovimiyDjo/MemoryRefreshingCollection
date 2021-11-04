using module "..\Classes\ProcessRunner.psm1"

Describe "ProcessRunner" {
    It "Provides correct output when process runs without error" {
        $processRunner = [ProcessRunner]::new()

        $processPath = "cmd.exe"
        $arguments = @("/c", "echo some output")
        $output = $processRunner.ExecuteProcess($processPath, $arguments, 0) *>&1
        $output | Should -BeExactly "some output"
    }

    It "Logs received output to error and throws when process exits with error" {
        $processRunner = [ProcessRunner]::new()
        $processRunner.Logger.Verbosity = "Error"

        $processPath = "cmd.exe"
        $arguments = @("/c", "echo some output before error & exit 1")
        { $processRunner.ExecuteProcess($processPath, $arguments, 0) *>&1 } | Should -Throw "ExecuteProcess failed: ExitCode='1'"

        $outputFile = "$TestDrive\beforeErrorOutput.txt"
        try {
            $processRunner.ExecuteProcess($processPath, $arguments, 0) *>$outputFile
        } catch {
            $null = "IgnoreCatch"
        }
        Get-Content -Path $outputFile -Raw | Should -BeLikeExactly "* ERROR Total ProcessRunner output:*some output before error*"
    }

    It "Logs received output to error and throws when process times out" {
        $processRunner = [ProcessRunner]::new()
        $processRunner.Logger.Verbosity = "Error"

        $processPath = "cmd.exe"
        $arguments = @("/c", "echo some output before timeout & sleep 5")
        { $processRunner.ExecuteProcess($processPath, $arguments, 1) *>&1 } | Should -Throw "Process exceeded timeout (1s)."

        $outputFile = "$TestDrive\beforeTimeoutOutput.txt"
        try {
            $processRunner.ExecuteProcess($processPath, $arguments, 2) *>$outputFile
        } catch {
            $null = "IgnoreCatch"
        }
        Get-Content -Path $outputFile -Raw | Should -BeLikeExactly "* ERROR Total ProcessRunner output:*some output before timeout*"
    }
}