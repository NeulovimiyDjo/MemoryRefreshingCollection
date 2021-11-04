using module "..\Classes\Logger.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class RemoteHostInteractor {
    [AllowNull()][PSCredential]$Credential
    [bool]$UseSSL
    [bool]$IgnoreSSLCert

    [ValidateNotNull()][Logger]$Logger
    [ValidateNotNullOrEmpty()][string]$RemoteHelperFunctionsFileContent
    [ValidateNotNull()][System.Collections.Generic.Dictionary[string, System.Management.Automation.Runspaces.PSSession]]$HostsSessions

    RemoteHostInteractor([PSCustomObject]$config) {
        $this.Init($config, (New-Object Logger([Verbosity]::Trace)))
    }
    RemoteHostInteractor([PSCustomObject]$config, [Logger]$logger) {
        $this.Init($config, $logger)
    }
    hidden [void]Init([PSCustomObject]$config, [Logger]$logger) {
        $this.Credential = $config.Credential
        $this.UseSSL = $config.UseSSL
        $this.IgnoreSSLCert = $config.IgnoreSSLCert

        $this.Logger = $logger
        $this.RemoteHelperFunctionsFileContent = Get-Content "$PSScriptRoot\RemoteHelperFunctions.ps1" -Raw
        $this.HostsSessions = @{}
    }

    [void]ExecuteRemotely([ScriptBlock]$scriptBlock, [object[]]$params, [string[]]$hostNamesList) {
        if ([RemoteHostInteractor]::IsLocalHost($hostNamesList)) {
            $this.Logger.Trace("Executing script block locally")
            $output = & $scriptBlock @params
            $this.Logger.Trace("Executed script block output: '$output'")
        } else {
            $this.Logger.Trace("Executing script block remotely on hosts $([Logger]::DisplayArray($hostNamesList))")
            $sessions = $this.GetOrCreateSessions($hostNamesList)
            $output = Invoke-Command -Session $sessions -ScriptBlock $scriptBlock -ArgumentList $params
            $this.Logger.Trace("Executed script block output: '$output'")
        }
    }

    [void]CopyToRemote([string]$localPath, [string]$toRemotePath, [string[]]$hostNamesList) {
        if ([RemoteHostInteractor]::IsLocalHost($hostNamesList)) {
            $this.Logger.Trace("Copying locally '$localPath' to '$toRemotePath'")
            Copy-Item -Path $localPath -Destination $toRemotePath -Force -Recurse
        } else {
            $this.Logger.Trace("Copying remotely '$localPath' to '$toRemotePath' on hosts $([Logger]::DisplayArray($hostNamesList))")
            $sessionsList = $this.GetOrCreateSessions($hostNamesList)
            foreach ($session in $sessionsList) {
                $this.Logger.Trace("Copying to host '$($session.ComputerName)'")
                Copy-Item -ToSession $session -Path $localPath -Destination $toRemotePath -Force -Recurse
            }
        }
    }

    [System.Management.Automation.Runspaces.PSSession[]]GetOrCreateSessions([string[]]$hostNamesList) {
        $sessionsList = @()
        foreach ($hostName in $hostNamesList) {
            if ($this.HostsSessions.ContainsKey($hostName)) {
                $sessionsList += $this.HostsSessions[$hostName]
            } else {
                $session = $this.CreateSession($hostName)
                $this.HostsSessions.Add($hostName, $session)
                $sessionsList += $session
            }
        }
        return $sessionsList
    }

    [System.Management.Automation.Runspaces.PSSession]CreateSession([string]$hostName) {
        $this.Logger.Trace("Creating WinRM session to '$hostName'")
        $params = @{
            ComputerName = $hostName
        }
        if ($null -ne $this.Credential) {
            $this.Logger.Trace("Using credential: UserName='$($this.Credential.UserName)'")
            $params.Credential = $this.Credential
        }
        if ($this.UseSSL) {
            $this.Logger.Trace("Using SSL")
            $params.UseSSL = $true
            if ($this.IgnoreSSLCert) {
                $this.Logger.Trace("Ignoring SSL certificate")
                $sessionOption = New-PSSessionOption -SkipCACheck -SkipCNCheck -SkipRevocationCheck
                $params.SessionOption = $sessionOption
            }
        }
        $session = New-PSSession @params

        $this.Logger.Trace("Loading helper functions to created session")
        Invoke-Command -Session $session -ScriptBlock ([ScriptBlock]::Create($this.RemoteHelperFunctionsFileContent))
        return $session
    }

    [void]CloseConnections() {
        $this.HostsSessions.Values | Remove-PSSession
        $this.HostsSessions.Clear()
    }

    static [bool]IsLocalHost([string[]]$hostNamesList) {
        if (($hostNamesList | Measure-Object).Count -eq 1 -and
            $hostNamesList[0] -eq "localhost") {
            return $true
        } else {
            return $false
        }
    }
}
