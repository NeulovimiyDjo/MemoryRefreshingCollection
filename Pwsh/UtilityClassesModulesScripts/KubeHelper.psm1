using module "..\Modules\Logger.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class KubeHelper {
    [ValidateNotNullOrEmpty()][string]$Namespace
    [ValidateNotNullOrEmpty()][string]$KubectlPath
    [ValidateNotNullOrEmpty()][string]$DockerCredsConfigPath

    [ValidateNotNull()][Logger]$Logger

    KubeHelper([PSCustomObject]$config, [Logger]$logger) {
        $this.Namespace = $config.Namespace
        $this.KubectlPath = $config.KubectlPath
        $this.DockerCredsConfigPath = "/tmp/helm/myapp/charts/subchart1/templates/dockercreds-secret.yaml"

        $this.Logger = $logger
    }

    [void]SetClusterLoginInfo([string]$server, [string]$token, [string]$caFilePath) {
        $this.Logger.Info("Setting Cluster login info")
        $clusterName = "current-cluster"
        $credsName = "current-creds"
        $contextName = "$credsName@$clusterName"

        if ($caFilePath) {
            $null = & $this.KubectlPath config set-cluster $clusterName --server=$server --certificate-authority=$caFilePath --embed-certs
        } else {
            $null = & $this.KubectlPath config set-cluster $clusterName --server=$server
        }
        $null = & $this.KubectlPath config set-credentials $credsName --token=$token
        $null = & $this.KubectlPath config set-context $contextName --cluster=$clusterName --user=$credsName --namespace=$this.Namespace
        $null = & $this.KubectlPath config use-context $contextName
    }

    [void]GenerateDockerSecretConfig([string]$server, [string]$user, [string]$password) {
        $this.Logger.Info("Generating docker secret config")
        $null = & $this.KubectlPath create secret docker-registry my-dockercreds -n $this.Namespace `
            --docker-server=$server `
            --docker-username=$user `
            --docker-password=$password `
            --docker-email=unused `
            --dry-run=client -o yaml |
            Out-File -FilePath $this.DockerCredsConfigPath -Force -Encoding utf8
        if ($LASTEXITCODE -ne 0) { throw "Generate docker secret config failed" }
    }

    [void]LinkDockerSecret() {
        if (Test-Path -Path $this.DockerCredsConfigPath) {
            $this.Logger.Info("Linking docker secret to default service account")
            $null = & $this.KubectlPath patch serviceaccount default -n $this.Namespace `
                -p "imagePullSecrets:`n - name: my-dockercreds"
            #-p ('{"imagePullSecrets": [{"name": "my-dockercreds"}]}' | ConvertTo-Json)
            if ($LASTEXITCODE -ne 0) { throw "Link docker secret failed" }
        } else {
            $this.Logger.Info("No generated dockercreds config file found. Skipping linking docker secret to default service account")
        }
    }

    [void]PatchWebServiceApp([string]$app, [bool]$throwOnError) {
        $this.Logger.Info("Patching web service app to '$app'")
        $null = & $this.KubectlPath patch service myapp -n $this.Namespace `
            -p "spec:`n selector:`n  app: myapp-$app"
        if ($LASTEXITCODE -ne 0) {
            $errMsg = "Patching web service app failed"
            if ($throwOnError){
                throw $errMsg
            } else {
                $this.Logger.Warn($errMsg)
            }
        }
    }

    [void]CheckClusterAccess() {
        $this.Logger.Debug("Checking cluster access")
        $null = & $this.KubectlPath get pods -n $this.Namespace
        if ($LASTEXITCODE -ne 0) { throw "Check cluster access failed" }
        $this.Logger.Trace("Check cluster access succeeded")
    }

    [void]ShowCurrentPods([string[]]$params) {
        $this.Logger.Debug("Currently active pods:")
        $pods = & $this.KubectlPath get pods @params -n $this.Namespace
        $this.Logger.Trace($pods)
    }

    [void]ShowLastEvents([int]$count) {
        $this.Logger.Debug("Last $count events:")
        $events = & $this.KubectlPath get events -n $this.Namespace --sort-by='.lastTimestamp' | Select-Object -Last $count
        $this.Logger.Trace($events)
    }

    [void]ScaleAllDeploymentsToZero([string[]]$params) {
        $this.Logger.Debug("Scaling all deployments to zero")
        $currentDeployments = & $this.KubectlPath get deployments @params -n $this.Namespace --no-headers=true --output=name
        if (($currentDeployments | Measure-Object).Count -gt 0) {
            & $this.KubectlPath scale deployment --replicas=0 @params -n $this.Namespace
            if ($LASTEXITCODE -ne 0) { throw "Scale deployment failed" }
        } else {
            $this.Logger.Warn("No deployments were found, skipping scale command")
        }
    }

    [void]RestartAllDeployments([string[]]$params) {
        $this.Logger.Debug("Restarting all deployments")
        $deploymentNames = & $this.KubectlPath get deployments @params -n $this.Namespace --no-headers -o 'custom-columns=NAME:.metadata.name'
        $deploymentNames -split '\r?\n' | ForEach-Object {
            $name = $_.Trim()
            $this.Logger.Debug("Restarting deployment '$name'")
            $null = & $this.KubectlPath rollout restart deployment $name -n $this.Namespace
            if ($LASTEXITCODE -ne 0) { throw "Restart deployment failed" }
        }
    }

    [void]RestartDeployment([string]$deploymentName) {
        $deploymentNames = & $this.KubectlPath get deployments -n $this.Namespace --no-headers -o 'custom-columns=NAME:.metadata.name'
        $deploymentExists = ($deploymentNames -split '\r?\n' | Where-Object { $_.Trim() -eq $deploymentName } | Measure-Object).Count -ge 1

        if ($deploymentExists) {
            $this.Logger.Debug("Restarting deployment '$deploymentName'")
            $null = & $this.KubectlPath rollout restart deployment $deploymentName -n $this.Namespace
            if ($LASTEXITCODE -ne 0) { throw "Restart deployment failed" }
        } else {
            $this.Logger.Warn("Deployment '$deploymentName' doesn't exist, skipping restart")
        }
    }

    [void]UpgradeHelmChart([string]$releaseName, [string]$chartPath, [string[]]$params) {
        $this.Logger.Debug("Upgrading helm release '$releaseName' from chart '$chartPath'")
        helm upgrade --install $releaseName $chartPath -n $this.Namespace @params
        if ($LASTEXITCODE -ne 0) { throw "Helm upgrade failed" }
    }

    [PSCustomObject[]]GetPods([string[]]$params) {
        $this.Logger.Trace("Getting pods params=$([Logger]::DisplayArray($params))")
        $podsLines = & $this.KubectlPath get pods -n $this.Namespace @params --no-headers -o 'custom-columns=NAME:.metadata.name,STATUS:.status.phase,UNREADYCONTAINERS:.status.containerStatuses[?(@.ready==false)].name,RESTARTINGCONTAINERS:.status.containerStatuses[?(@.restartCount>=3)].name,custom-columns=DELETIONTIMESTAMP:.metadata.deletionTimestamp'
        $pods = $podsLines -split '\r?\n' | ForEach-Object {
            $output = $_.Trim()
            $podName = ($output -split "\s+")[0]
            $podStatus = ($output -split "\s+")[1]
            $podUnreadyContainers = ($output -split "\s+")[2]
            $podRestartingContainers = ($output -split "\s+")[3]
            $podDeletionTimestamp = ($output -split "\s+")[4]
            [PSCustomObject]@{
                Name = $podName
                Status = $podStatus
                Ready = $podUnreadyContainers -eq '<none>'
                IsCrashRestarting = $podRestartingContainers -ne '<none>'
                IsTerminating = $podDeletionTimestamp -ne '<none>'
            }
        }
        return $pods
    }

    [string]GetConfigmap([string]$configmapName, [string]$valuePath) {
        $this.Logger.Trace("Getting configmap '$configmapName', valuePath='$valuePath'")
        $res = & $this.KubectlPath get configmap $configmapName -n $this.Namespace -o go-template="{{ $valuePath }}"
        if ($LASTEXITCODE -ne 0) { throw "Get configmap failed" }
        return $res | Out-String
    }

    [string]GetSecret([string]$secretName, [string]$valuePath) {
        $this.Logger.Trace("Getting secret '$secretName', valuePath='$valuePath'")
        $res = & $this.KubectlPath get secret $secretName -n $this.Namespace -o go-template="{{ $valuePath | base64decode }}"
        if ($LASTEXITCODE -ne 0) { throw "Get secret failed" }
        return $res | Out-String
    }

#---------Usage_from_other_classes------------

    #WaitForAllPodsToBeStopped(@("-l", "my-chart=myapp"))
    hidden [void]WaitForAllPodsToBeStopped([string[]]$params) {
        $this.Logger.Debug("Waiting for all pods to be stopped")
        $scriptBlock = {
            if ($this.Count($this.GetPods($params)) -gt 0) {
                break
            }
            $this.ShowCurrentPods($params)
            $this.ShowLastEvents(5)
            $this.Logger.Debug("Not all pods are stopped. Waiting...")
        }
        $this.ExecuteInLoopWithTimeout($scriptBlock, 300)
    }

    hidden [void]WaitForAllPodsToSuccessfullyStart($params) {
        $this.Logger.Debug("Waiting for all pods to successfully start")
        $scriptBlock = {
            $pods = $this.GetPods($params)
            $this.EnsureNoFailedPods($pods)
            if ($this.AllPodsHaveSuccessfullyStarted($pods)) {
                break
            }
            $this.ShowCurrentPods($params)
            $this.ShowLastEvents(5)
            $this.Logger.Debug("Not all pods have successfully started. Waiting...")
        }
        $this.ExecuteInLoopWithTimeout($scriptBlock, 900)

        $this.Logger.Debug("All pods started successfully")
        $this.ShowCurrentPods($params)
    }

    hidden [void]EnsureNoFailedPods([PSCustomObject[]]$pods) {
        $failedPods = $pods | Where-Object { ($_.Status -eq "Failed" -and (-not $_.IsTerminating)) -or ($_.IsCrashRestarting) }
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
