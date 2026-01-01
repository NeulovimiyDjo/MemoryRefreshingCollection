$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function StartService() {
    Write-Host "Starting service"
    Start-Process -FilePath "my-service" -WorkingDirectory "/tmp" -ArgumentList "http://127.0.0.1:1234" -RedirectStandardOutput "/tmp/stdout.txt"

    $secondsPassed = 0
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    do {
        try { $tcpClient.Connect("127.0.0.1", 1234) } catch { $null = $null }
        Start-Sleep -Seconds 1
        $secondsPassed++
    } while (-not $tcpClient.Connected -and $secondsPassed -le 60)

    if (-not $tcpClient.Connected) {
        throw "Failed to start service"
    } else {
        $tcpClient.Dispose()
    }
}

function StopService() {
    Write-Host "Stopping service"
    Stop-Process -Name "my-service xxx:1234"
}

StartService
Get-Process
StopService
