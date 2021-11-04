using module "..\Classes\Logger.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class ProcessRunner {
    [ValidateNotNull()][Logger]$Logger
    hidden [int]$MinimalAllowedOutputAgeToBeProcessed = 50

    ProcessRunner() {
        $this.Init((New-Object Logger([Verbosity]::Trace)))
    }
    ProcessRunner([Logger]$logger) {
        $this.Init($logger)
    }
    hidden [void]Init([Logger]$logger) {
        $this.Logger = $logger
    }

    [string]ExecuteProcess([string]$processPath, [string[]]$arguments, [int]$timeout) {
        $this.Logger.Trace("Executing process='$processPath', arguments=$([Logger]::DisplayArray($arguments)), timeout='$timeout'")

        $procinfo = New-Object System.Diagnostics.ProcessStartInfo
        $procinfo.FileName = $processPath
        $procinfo.RedirectStandardError = $true
        $procinfo.RedirectStandardOutput = $true
        $procinfo.UseShellExecute = $false
        $procinfo.Arguments = $arguments
        $proc = New-Object System.Diagnostics.Process
        $proc.StartInfo = $procinfo

        $output = $this.ExecuteProcessReadingOutput($proc, $timeout)
        $this.Logger.Trace("ExitCode='$($proc.ExitCode)'")
        if ($proc.ExitCode -ne 0) {
            $this.Logger.Error("Total ProcessRunner output:`n$output")
            throw "ExecuteProcess failed: ExitCode='$($proc.ExitCode)'"
        }
        return $output
    }

    [string]ExecuteProcessReadingOutput([System.Diagnostics.Process]$proc, [int]$timeout) {
        $scopeRef = [PSCustomObject]@{
            TotalOutput = New-Object System.Text.StringBuilder
            OutputInfos = @{}
            StopWatch = New-Object System.Diagnostics.StopWatch
        }

        $outputReceived = {
            if (-not [String]::IsNullOrEmpty($EventArgs.Data)) {
                $outputInfo = [PSCustomObject]@{
                    OutputData = $EventArgs.Data.ToString().Trim()
                    ElapsedMillisecondsStamp = $Event.MessageData.StopWatch.ElapsedMilliseconds
                }
                $Event.MessageData.OutputInfos.Add($Event.EventIdentifier, $outputInfo)
            }
        }
        $outEvent = Register-ObjectEvent -InputObject $proc `
            -EventName 'OutputDataReceived' `
            -Action $outputReceived `
            -MessageData $scopeRef
        $errEvent = Register-ObjectEvent -InputObject $proc `
            -EventName 'ErrorDataReceived' `
            -Action $outputReceived `
            -MessageData $scopeRef

        $scopeRef.StopWatch.Start()
        $proc.Start() | Out-Null
        $proc.BeginOutputReadLine()
        $proc.BeginErrorReadLine()

        if ($timeout -le 0) { $timeout = [int]::MaxValue }
        $timedOut = $false
        $secondsPassed = 0
        while ($secondsPassed -lt $timeout -and -not $proc.HasExited) {
            $this.ProcessLastOutputsInOrder($scopeRef)
            Start-Sleep -Seconds 1
            $secondsPassed++
        }

        if (-not $proc.HasExited) {
            $proc.Kill()
            $timedOut = $true
        }

        Unregister-Event -SourceIdentifier $outEvent.Name
        Unregister-Event -SourceIdentifier $errEvent.Name

        Start-Sleep -Milliseconds $this.MinimalAllowedOutputAgeToBeProcessed
        $this.ProcessLastOutputsInOrder($scopeRef)
        $scopeRef.StopWatch.Stop()
        if ($timedOut) {
            $this.Logger.Error("Total ProcessRunner output:`n$($scopeRef.TotalOutput.ToString().Trim())")
            throw "Process exceeded timeout ($($timeout)s)."
        } else {
            return $scopeRef.TotalOutput.ToString().Trim()
        }
    }

    [void]ProcessLastOutputsInOrder([PSCustomObject]$scopeRef) {
        $orderedOutputInfosKeys = $scopeRef.OutputInfos.Keys | Sort-Object
        $processedOutputInfosKeys = @()
        foreach ($eventId in $orderedOutputInfosKeys) {
            $outputInfo = $scopeRef.OutputInfos[$eventId]
            if ($scopeRef.StopWatch.ElapsedMilliseconds - $outputInfo.ElapsedMillisecondsStamp -ge $this.MinimalAllowedOutputAgeToBeProcessed) {
                $scopeRef.TotalOutput.AppendLine($outputInfo.OutputData)
                $this.Logger.Trace("ProcessRunner: $($outputInfo.OutputData)")
                $processedOutputInfosKeys += $eventId
            }
        }
        foreach ($eventId in $processedOutputInfosKeys) {
            $outputInfo = $scopeRef.OutputInfos.Remove($eventId)
        }
    }
}