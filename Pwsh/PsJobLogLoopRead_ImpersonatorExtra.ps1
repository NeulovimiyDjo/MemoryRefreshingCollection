 
function StartAndWaitProcessWithLogonWhileInteractivelyReadingLog([Logger]$logger, [string]$userName, [string]$domain, [string]$password, [string]$binary, [string]$arguments, [int]$timeoutSeconds, [string]$logPath) {
    $execProcessJob = Start-Job {
        Add-Type -TypeDefinition $using:createProcessCode -Language CSharp
        $executor = New-Object CreateProcessWithLogon
        return $executor.CreateProcess(
            $using:userName,
            $using:domain,
            $using:password,
            [CreateProcessWithLogon+LogonFlags]::LOGON_NETCREDENTIALS_ONLY,
            $using:binary,
            $using:arguments,
            [System.IO.Path]::GetDirectoryName($using:binary),
            $using:timeoutSeconds * 1000);
    }

    $readFromPos = 0
    $logger.Debug("Reading log file until process job is running")
    while ($execProcessJob.State -eq "Running") {
        if (Test-Path -Path $logPath) {
            Get-Content $logPath -Raw -Encoding utf8 | ForEach-Object {
                if ($readFromPos -lt $_.Length) {
                    Write-Host -NoNewline $logger.Sanitize($_.Substring($readFromPos, $_.Length - $readFromPos))
                    $readFromPos = $_.Length
                }
            }
        } else {
            $logger.Debug("Log file does not exist yet")
        }
        Start-Sleep -Seconds 5
    }

    Write-Host "`n"
    $logger.Debug("Receiving process job")
    $res = $execProcessJob | Receive-Job -Wait
    $execProcessJob | Remove-Job
    return $res
}

    
[string]ExecuteProcessAsUserWhileInteractivelyReadingLog([string]$processPath, [string[]]$arguments, [int]$timeout, [string]$logPath) {
    $this.Logger.Trace("Executing as user='$($this.UserName)', process='$processPath', arguments=$([Logger]::DisplayArray($arguments)), timeout='$timeout'")
    $result = StartAndWaitProcessWithLogonWhileInteractivelyReadingLog $this.Logger $this.UserNameOnly $this.Domain $this.Password $processPath $arguments $timeout $logPath
    $this.Logger.Trace($result.Output.Trim())
    $this.Logger.Trace("ExitCode='$($result.ExitCode)'")
    if ($result.ExitCode -ne 0) {
        $this.Logger.Error("Total ProcessRunner output:`n$($result.Output)")
        throw "ExecuteProcessAsUser failed: ExitCode='$($result.ExitCode)'"
    }
    return $result.Output.Trim()
}
}
