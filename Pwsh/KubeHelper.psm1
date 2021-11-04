using module "..\Modules\Logger.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class KubeHelper {
    [ValidateNotNullOrEmpty()][string]$Namespace
    [ValidateNotNullOrEmpty()][string]$KubectlPath

    [ValidateNotNull()][Logger]$Logger

    KubeHelper([PSCustomObject]$config, [Logger]$logger) {
        $this.Namespace = $config.Namespace
        $this.KubectlPath = $config.KubectlPath

        $this.Logger = $logger
    }

    [void]CheckClusterAccess() {
        $this.Logger.Debug("Checking cluster access")
        $null = & $this.KubectlPath get pods -n $this.Namespace
        if ($LASTEXITCODE -ne 0) { throw "Check cluster access failed" }
        $this.Logger.Trace("Check cluster access succeeded")
    }

    [void]ShowCurrentPods() {
        $this.Logger.Debug("Currently active pods:")
        $pods = & $this.KubectlPath get pods -n $this.Namespace
        $this.Logger.Trace($pods)
    }

    [void]ShowLastEvents([int]$count) {
        $this.Logger.Debug("Last $count events:")
        $events = & $this.KubectlPath get events -n $this.Namespace --sort-by='.lastTimestamp' | Select-Object -Last $count
        $this.Logger.Trace($events)
    }

    [void]ScaleAllDeploymentsToZero() {
        $this.Logger.Debug("Scaling all deployments to zero")
        $currentDeployments = & $this.KubectlPath get deployments --no-headers=true --output=name -n $this.Namespace
        if (($currentDeployments | Measure-Object).Count -gt 0) {
            & $this.KubectlPath scale deployment --replicas=0 --all -n $this.Namespace
            if ($LASTEXITCODE -ne 0) { throw "Scale deployment failed" }
        } else {
            $this.Logger.Warn("No deployments were found, skipping scale command")
        }
    }

    [void]RestartAllDeployments() {
        $this.Logger.Debug("Restarting all deployments")
        $null = & $this.KubectlPath rollout restart deployments -n $this.Namespace
        if ($LASTEXITCODE -ne 0) { throw "Restart deployment failed" }
    }

    [void]UpgradeHelmChart([string]$chartName, [string]$chartPath, [string[]]$params) {
        $this.Logger.Debug("Upgrading helm chart '$chartName'")
        helm upgrade --install $chartName $chartPath -n $this.Namespace @params
        if ($LASTEXITCODE -ne 0) { throw "Helm upgrade failed" }
    }

    [PSCustomObject[]]GetPods([string[]]$params) {
        $this.Logger.Trace("Getting pods params=$([Logger]::DisplayArray($params))")
        $podsLines = & $this.KubectlPath get pods -n $this.Namespace @params --no-headers -o 'custom-columns=NAME:.metadata.name,STATUS:.status.phase,UNREADYCONTAINERS:.status.containerStatuses[?(@.ready==false)].name,RESTARTINGCONTAINERS:.status.containerStatuses[?(@.restartCount>=3)].name'
        $pods = $podsLines -split '\r?\n' | ForEach-Object {
            $output = $_.Trim()
            $podName = ($output -split "\s+")[0]
            $podStatus = ($output -split "\s+")[1]
            $podUnreadyContainers = ($output -split "\s+")[2]
            $podRestartingContainers = ($output -split "\s+")[3]
            [PSCustomObject]@{
                Name = $podName
                Status = $podStatus
                Ready = $podUnreadyContainers -eq '<none>'
                IsCrashRestarting = $podRestartingContainers -ne '<none>'
            }
        }
        return $pods
    }

    [string]GetSecret([string]$secretName, [string]$valuePath) {
        $this.Logger.Trace("Getting secret '$secretName', valuePath='$valuePath'")
        $res = & $this.KubectlPath get secret $secretName -n $this.Namespace -o go-template="{{ $valuePath | base64decode }}"
        if ($LASTEXITCODE -ne 0) { throw "Get secret failed" }
        return $res | Out-String
    }

#---------------------
    
    hidden [void]WaitForAllPodsToBeStopped() {
        $this.Logger.Debug("Waiting for all pods to be stopped")
        $scriptBlock = {
            if ($this.Count($this.KubeHelper.GetPods(@())) -gt 0) {
                break
            }
            $this.KubeHelper.ShowCurrentPods()
            $this.KubeHelper.ShowLastEvents(5)
            $this.Logger.Debug("Not all pods are stopped. Waiting...")
        }
        $this.ExecuteInLoopWithTimeout($scriptBlock, 300)
    }

    hidden [void]WaitForAllDeploymentsToSuccessfullyStart() {
        $this.Logger.Debug("Waiting for all deployment pods to successfully start")
        $scriptBlock = {
            $pods = $this.KubeHelper.GetPods(@())
            $this.EnsureNoFailedPods($pods)
            if ($this.AllPodsHaveSuccessfullyStarted($pods)) {
                break
            }
            $this.KubeHelper.ShowCurrentPods()
            $this.KubeHelper.ShowLastEvents(5)
            $this.Logger.Debug("Not all deployment pods have successfully started. Waiting...")
        }
        $this.ExecuteInLoopWithTimeout($scriptBlock, 900)
    }

    hidden [void]EnsureNoFailedPods([PSCustomObject[]]$pods) {
        $failedPods = $pods | Where-Object { ($_.Status -eq "Failed") -or ($_.IsCrashRestarting) }
        if (($failedPods | Measure-Object).Count -gt 0) {
            foreach ($pod in $failedPods) {
                $this.Logger.Error("Pod '$($pod.Name)' failed with status '$($pod.Status)' (IsCrashRestarting=$($pod.IsCrashRestarting))")
            }
            throw "Some pods failed to start"
        }
    }

    hidden [bool]AllPodsHaveSuccessfullyStarted([PSCustomObject[]]$pods) {
        $unreadyPods = $pods | Where-Object { ($_.Status -ne "Running") -or (-not $_.Ready) }
        if ($this.Count($unreadyPods) -gt 0) {
            foreach ($pod in $unreadyPods) {
                $this.Logger.Trace("Pod '$($pod.Name)' has not yet successfully started")
            }
            return $false
        }
        return $true
    }

    hidden [void]ExecuteInLoopWithTimeout([ScriptBlock]$scriptBlock, [int]$timeoutSec) {
        $secondsPassed = 0
        while ($true) {
            & $scriptBlock
            Start-Sleep 5
            $secondsPassed = $secondsPassed + 5
            if ($secondsPassed -ge $timeoutSec) {
                throw "Exceeded $($timeoutSec)s timeout"
            }
        }
    }

    hidden [int]Count($arr) {
        return ($arr | Measure-Object).Count
    }
}
