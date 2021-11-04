#Requires -RunAsAdministrator

#Example usage:
#&./CollectMemoryUsage.ps1
#&./CollectMemoryUsage.ps1 -logPath "./MemoryUsageLog.txt" -processName "Code" -sampleCount (4*60*10) -sleepInterval 15

param(
    [string]$logPath = "MemoryUsageLog.txt",
    [string]$processName = $null,
    [int]$sampleCount = 4*60*10,
    [int]$sleepInterval = 15
)

Write-Host "Started collecting memory usage. logPath=$logPath processName=$processName, sampleCount=$sampleCount, sleepInterval=$sleepInterval"

"---------------------------------------------------------`n" | Out-File -FilePath $logPath
for ($i = 0; $i -lt $sampleCount; $i++) {
    "Date=$(Get-Date)`n" | Out-File -Append -FilePath $logPath

    $totalCommit = ((Get-Counter -Counter "\Memory\Committed Bytes").CounterSamples.RawValue)/1024/1024
    $totalAvailable = ((Get-Counter -Counter "\Memory\Available Bytes").CounterSamples.RawValue)/1024/1024
    $totalCommitLimit = ((Get-Counter -Counter "\Memory\Commit Limit").CounterSamples.RawValue)/1024/1024
    $msg = "TotalCommit=$totalCommit TotalAvailable=$totalAvailable TotalCommitLimit=$totalCommitLimit"
    $msg | Out-File -Append -FilePath $logPath

    "`n" | Out-File -Append -FilePath $logPath
    if ($processName) {
        $processes = Get-Process | Where-Object { $_.ProcessName -eq $processName }
    } else {
        $processes = Get-Process
    }
    $processes | Sort-Object -Property PrivateMemorySize64 -Descending | Foreach-Object {
        $cmd = $_.CommandLine
        if (!$cmd) {
            $cmd = Get-CimInstance Win32_Process -Filter "ProcessId = '$($_.Id)'" -Property CommandLine | Select-Object -ExpandProperty CommandLine
        }
        $msg = "PID=$($_.Id) Commit=$($_.PrivateMemorySize64/1024/1024) WorkingSet=$($_.WorkingSet64/1024/1024) Cmd=$cmd`n"
        $msg | Out-File -Append -FilePath $logPath
    }
    
    "---------------------------------------------------------`n" | Out-File -Append -FilePath $logPath

    if (($i+1) % (60/$sleepInterval) -eq 0) {
        Write-Host "$($i+1)/$sampleCount samples collected"
    }
    Start-Sleep $sleepInterval
}