$ErrorActionPreference = "Stop"
$global:ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$timeout = 60
$hostName = [System.Net.Dns]::GetHostName()

function LogMessage([string]$msg) {
    Write-Host "MESSAGE ON HOST '$($hostName)': $msg"
}

#region Web services control
function CreateWebServiceIfNotExist([PSCustomObject]$webService, [string]$webServicesRoot) {
    Import-Module WebAdministration
    if (-not (AppPoolExists $webService.Config.PoolName) -or -not (WebAppExists $webService.Path)) {
        LogMessage "Creating web service '$($webService.Path)'"
        if ($webService.Config.PoolUserName) {
            SetFullFilePermissions "$webServicesRoot\$($webService.Path)" $webService.Config.PoolUserName
        } else {
            SetFullFilePermissions "$webServicesRoot\$($webService.Path)" "IIS_IUSRS"
        }
        CreateAppPoolIfNotExist $webService.Config
        CreateWebAppIfNotExist $webService.Path $webService.Config
        StopAppPool $webService.Config.PoolName
    }
}

function CreateAppPoolIfNotExist([PSCustomObject]$webServiceConfig) {
    Import-Module WebAdministration
    if (-not (AppPoolExists $webServiceConfig.PoolName)) {
        LogMessage "Creating app pool '$($webServiceConfig.PoolName)'"
        New-WebAppPool -Name $webServiceConfig.PoolName -Force
        Set-ItemProperty -Path "IIS:\AppPools\$($webServiceConfig.PoolName)" -Name managedRuntimeVersion -Value $webServiceConfig.PoolManagedRuntimeVersion | Out-Null
        if ($webServiceConfig.PoolUserName) {
            Set-ItemProperty -Path "IIS:\AppPools\$($webServiceConfig.PoolName)" -Name processModel.identityType -Value "SpecificUser" | Out-Null
            Set-ItemProperty -Path "IIS:\AppPools\$($webServiceConfig.PoolName)" -Name processModel.userName -Value $webServiceConfig.PoolUserName | Out-Null
            Set-ItemProperty -Path "IIS:\AppPools\$($webServiceConfig.PoolName)" -Name processModel.password -Value $webServiceConfig.PoolPassword | Out-Null
        }
    } else {
        LogMessage "App pool '$($webServiceConfig.PoolName)' already exists"
    }
}

function CreateWebAppIfNotExist([string]$appPath, [PSCustomObject]$webServiceConfig) {
    Import-Module WebAdministration
    if (-not (WebAppExists $appPath)) {
        LogMessage "Creating web app '$appPath'"
        ConvertTo-WebApplication -ApplicationPool $webServiceConfig.poolName -PSPath "IIS:\Sites\Default Web Site\$appPath"
        $siteLocation = "Default Web Site/$($appPath.Replace("\", "/"))"
        Set-WebConfiguration -Location $siteLocation -Filter 'system.webserver/security/access' -Value $webServiceConfig.SslFlags
        Set-WebConfiguration -Location $siteLocation -Filter 'system.webserver/security/authentication/anonymousAuthentication' -Value @{ enabled = $webServiceConfig.anonymousAuthentication }
        Set-WebConfiguration -Location $siteLocation -Filter 'system.webserver/security/authentication/windowsAuthentication' -Value @{ enabled = $webServiceConfig.windowsAuthentication }
    } else {
        LogMessage "Web app '$appPath' already exists"
    }
}

function StartAppPool([string]$poolName) {
    Import-Module WebAdministration
    if (-not (AppPoolExists $poolName)) {
        throw "Unable to start app pool '$poolName' because it doesn't exist"
    }

    if (-not (AppPoolIsStarted $poolName)) {
        LogMessage "Starting app pool '$poolName'"
        Start-WebAppPool -Name $poolName
        $secondsPassed = 0
        while (-not (AppPoolIsStarted $poolName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (Get-WebAppPoolState -Name $poolName).Value
                throw "Start app pool timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "App pool '$poolName' is already started"
    }
}

function StopAppPool([string]$poolName) {
    Import-Module WebAdministration
    if (-not (AppPoolExists $poolName)) {
        LogMessage "Unable to stop app pool '$poolName' because it doesn't exist"
        return
    }

    if (-not (AppPoolIsStopped $poolName)) {
        LogMessage "Stopping app pool '$poolName'"
        Stop-WebAppPool -Name $poolName
        $secondsPassed = 0
        while (-not (AppPoolIsStopped $poolName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (Get-WebAppPoolState -Name $poolName).Value
                throw "Stop app pool timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "App pool '$poolName' is already stopped"
    }
}

function DeleteIISWebAppConfigEntries([string[]]$webAppsToDeletePaths) {
	Write-Host "Deleting web apps config locations if exist"
	[System.Reflection.Assembly]::LoadFrom("C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll") | Out-Null
	$sm = [Microsoft.Web.Administration.ServerManager]::new()
	$iisConfig = $sm.GetApplicationHostConfiguration()
	foreach ($appName in $webAppsToDeletePaths) {
		$iisConfig.RemoveLocationPath("Default Web Site/$($appName.Replace("\", "/"))")
	}
	$sm.CommitChanges()
}

function AppPoolExists([string]$poolName) {
    Import-Module WebAdministration
    return (Test-Path "IIS:\AppPools\$poolName") -eq $true
}

function WebAppExists([string]$appPath) {
    Import-Module WebAdministration
    return $null -ne (Get-WebApplication -Name $appPath.Replace("\", "/"))
}

function AppPoolIsStarted([string]$poolName) {
    Import-Module WebAdministration
    return (Get-WebAppPoolState -Name $poolName).Value -eq "Started"
}

function AppPoolIsStopped([string]$poolName) {
    Import-Module WebAdministration
    return (Get-WebAppPoolState -Name $poolName).Value -eq "Stopped"
}
#endregion

#region Win services control
function CreateWinServiceIfNotExist([PSCustomObject]$winService, [string]$winServicesRoot) {
    if (-not (WinServiceExists $winService.Config.ServiceName)) {
        LogMessage "Creating win service '$($winService.Path)'"
        if ($winService.Config.ServiceUserName) {
            SetFullFilePermissions "$winServicesRoot\$($winService.Path)" $winService.Config.ServiceUserName
        }
        $params = @{
            Name = $winService.Config.ServiceName
            BinaryPathName = "$winServicesRoot\$($winService.Path)\$($winService.Config.RelativeBinaryPathWithArgs)"
            DisplayName = $winService.Config.DisplayName
            Description = $winService.Config.Description
            StartupType = $winService.Config.StartupType
        }
        if ($winService.Config.ServiceUserName) {
            $securePassword = ConvertTo-SecureString -String $winService.Config.ServicePassword -AsPlainText -Force
            $credential = New-Object PSCredential $winService.Config.ServiceUserName, $securePassword
            $params["Credential"] = $credential
        }
        New-Service @params | Out-Null
        StopWinService $winService.Config.ServiceName
    }
}

function StartWinService([string]$serviceName) {
    if (-not (WinServiceExists $serviceName)) {
        throw "Unable to start win service '$serviceName' because it doesn't exist"
    }

    if (WinServiceIsDisabled $serviceName) {
        LogMessage "Skipping start win service '$serviceName' because it's disabled"
        return
    }

    if (-not (WinServiceIsStarted $serviceName)) {
        LogMessage "Starting win service '$serviceName'"
        (Get-Service -Name $serviceName -ErrorAction "Stop").Start()
        $secondsPassed = 0
        while (-not (WinServiceIsStarted $serviceName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue").Status
                throw "Start win service timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "Win service '$serviceName' is already started"
    }
}

function StopWinService([string]$serviceName) {
    if (-not (WinServiceExists $serviceName)) {
        LogMessage "Unable to stop win service '$serviceName' because it doesn't exist"
        return
    }

    if (-not (WinServiceIsStopped $serviceName)) {
        LogMessage "Stopping win service '$serviceName'"
        (Get-Service -Name $serviceName -ErrorAction "Stop").Stop()
        $secondsPassed = 0
        while (-not (WinServiceIsStopped $serviceName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue").Status
                throw "Stop win service timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "Win service '$serviceName' is already stopped"
    }
}

function WinServiceExists([string]$serviceName) {
    return $null -ne (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue")
}

function WinServiceIsStarted([string]$serviceName) {
    return (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue").Status -eq "Running"
}

function WinServiceIsStopped([string]$serviceName) {
    return (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue").Status -eq "Stopped"
}

function WinServiceIsDisabled([string]$serviceName) {
    return (Get-Service -Name $serviceName -ErrorAction "SilentlyContinue").StartType -eq "Disabled"
}
#endregion

#region Cluster services control
function CreateClusterServiceIfNotExist([string]$serviceName, [string]$displayName) {
    if (-not (ClusterServiceExists $serviceName)) {
        LogMessage "Creating cluster service '$serviceName'"
        $groupResource = Get-ClusterResource | Where-Object { $_.Name -eq $_.OwnerGroup.Name -and $_.ResourceType.Name -eq "Network Name" }
        $createdResource = Add-ClusterResource -Name $displayName -Group $groupResource.OwnerGroup -ResourceType "Generic Service"
        Set-ClusterParameter -InputObject $createdResource -Name "ServiceName" -Value $serviceName
        Add-ClusterResourceDependency -InputObject $createdResource -Provider $groupResource
        StopClusterService $serviceName
    }
}

function StartClusterService([string]$serviceName) {
    if (-not (ClusterServiceExists $serviceName)) {
        throw "Unable to start cluster service '$serviceName' because it doesn't exist"
    }

    if (-not (ClusterServiceIsStarted $serviceName)) {
        LogMessage "Starting cluster service '$serviceName'"
        GetClusterResource $serviceName | Start-ClusterResource
        $secondsPassed = 0
        while (-not (ClusterServiceIsStarted $serviceName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (GetClusterResource $serviceName).State
                throw "Start cluster service timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "Cluster service '$serviceName' is already started"
    }
}

function StopClusterService([string]$serviceName) {
    if (-not (ClusterServiceExists $serviceName)) {
        LogMessage "Unable to stop cluster service '$serviceName' because it doesn't exist"
        return
    }

    if (-not (ClusterServiceIsStopped $serviceName)) {
        LogMessage "Stopping cluster service '$serviceName'"
        GetClusterResource $serviceName | Stop-ClusterResource
        $secondsPassed = 0
        while (-not (ClusterServiceIsStopped $serviceName)) {
            Start-Sleep -Seconds 1
            $secondsPassed++
            if ($secondsPassed -gt $timeout) {
                $currentStatus = (GetClusterResource $serviceName).State
                throw "Stop cluster service timeout exceeded ($($timeout)s), current status is '$currentStatus'"
            }
        }
    } else {
        LogMessage "Cluster service '$serviceName' is already stopped"
    }
}

function ClusterServiceExists([string]$serviceName) {
    return $null -ne (GetClusterResource $serviceName)
}

function ClusterServiceIsStarted([string]$serviceName) {
    return (GetClusterResource $serviceName).State -eq "Online"
}

function ClusterServiceIsStopped([string]$serviceName) {
    return (GetClusterResource $serviceName).State -eq "Offline"
}

function GetClusterResource([string]$serviceName) {
    return (Get-ClusterResource | Get-ClusterParameter | Where-Object { $_.Name -eq "ServiceName" -and $_.Value -eq $serviceName }).ClusterObject
}
#endregion

#region Files
function SetFullFilePermissions([string]$path, [string]$userName) {
    LogMessage "Setting file permissions for path '$path' to '$userName'"
    $acl = Get-Acl $path
    $ar = New-Object System.Security.AccessControl.FileSystemAccessRule($userName, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($ar)
    Set-Acl $path $acl | Out-Null
}

function BackupDir([string]$dir, [string]$backupsDir) {
    if (-not (Test-Path -Path $dir)) {
        LogMessage "BackupDir: Directory '$dir' doesn't exist, skipping"
        return
    }

    $dirName = Split-Path -Path $dir -Leaf
    $dateTimeMark = (Get-Date).ToUniversalTime().ToString("yyyy_MM_dd_HH_mm_ss")
    $toDir = Join-Path -Path $backupsDir -ChildPath "$($dirName)_$($dateTimeMark)_UTC"
    LogMessage "Backing up directory '$dir' to '$toDir'"

    CreateDirIfNotExists $toDir
    Copy-Item -Path $dir -Destination $toDir -Force -Recurse
}

function RenameAllLockedFilesInDir([string]$dir) {
    if (-not (Test-Path -Path $dir)) {
        LogMessage "RenameAllLockedFilesInDir: Directory '$dir' doesn't exist, skipping"
        return
    }

    Get-ChildItem -Path $dir -File -Recurse -Include "*.dll", "*.exe" | ForEach-Object {
        $filePath = $_.FullName
        RenameFileIfLocked($filePath)
    }
}

function DeleteDirContentExceptExcludedRootItems([string]$dir, [string[]]$exclude) {
    if (-not (Test-Path -Path $dir)) {
        LogMessage "DeleteDirContentExceptExcludedRootItems: Directory '$dir' doesn't exist, skipping"
        return
    }

    Get-ChildItem -Path $dir -File -Exclude ($exclude + "*.lockedtmp") | Remove-Item -Recurse -Force
    Get-ChildItem -Path $dir -Directory -Exclude $exclude | ForEach-Object {
        $subDir = $_.FullName
        Get-ChildItem -Path $subDir -Recurse -File -Exclude "*.lockedtmp" | Remove-Item -Recurse -Force
        Get-ChildItem -Path $subDir -Recurse -File -Include "*.lockedtmp" | ForEach-Object {
            $filePath = $_.FullName
            if (-not (FileIsLocked $filePath)) {
                LogMessage "Deleting previously locked but now unlocked file '$filePath'"
                Remove-Item -Path $filePath -Force
            }
        }
    }

    # Try deleting everything in case there aren't any locked files
    try {
        Get-ChildItem -Path $dir -Exclude $exclude | Remove-Item -Recurse -Force -ErrorAction Stop
    } catch {
        LogMessage "Failed to delete everything in dir: '$dir', some files are still locked"
    }
}

function RenameFileIfLocked([string]$filePath) {
    if (FileIsLocked $filePath) {
        $renamedFilePath = $filePath + "." + (Get-Date).ToString("yyyyMMddHHmmss") + ".lockedtmp"
        Move-Item -Path $filePath -Destination $renamedFilePath
        LogMessage "File '$filePath' was locked and was renamed to '$renamedFilePath'"
    }
}

function FileIsLocked([string]$filePath) {
    $fileInfo = New-Object System.IO.FileInfo $filePath
    if ($fileInfo.IsReadonly -eq $true) {
        $fileInfo.IsReadonly = $false
    }
    try {
        $fileStream = $fileInfo.Open([System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
        if ($fileStream) {
            $fileStream.Close()
        }
        return $false
    } catch {
        # file is locked by a process.
        return $true
    }
}

function DeleteIfExists([string]$path) {
    if (Test-Path -Path $path) {
        LogMessage "Deleting '$path'"
        Remove-Item -Force -Path $path
    }
}

function CreateDirIfNotExists([string]$dir) {
    if (-not (Test-Path -Path $dir)) {
        LogMessage "Creating directory '$dir'"
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
}

function Zip([string]$path, [string]$toFile) {
    LogMessage "Zipping '$path' to '$toFile'"
    Compress-Archive -CompressionLevel Fastest -Path $path -DestinationPath $toFile
}

function Unzip([string]$file, [string]$toPath) {
    LogMessage "Unzipping '$file' to '$toPath'"
    Expand-Archive -Force -Path $file -DestinationPath $toPath
}
#endregion
