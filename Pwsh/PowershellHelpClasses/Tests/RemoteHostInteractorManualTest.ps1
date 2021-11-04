using module "..\Classes\RemoteHostInteractor.psm1"

$config = [PSCustomObject]@{
    Credential = $null
    UseSSL = $false
    IgnoreSSLCert = $false
}
$remoteHostInteractor = [RemoteHostInteractor]::new($config)

try {
    $remoteHostInteractor.ExecuteRemotely({
            param(
                [string]$SomeArg1,
                [string]$SomeArg2
            )

            Write-Host "HostName='$([System.Net.Dns]::GetHostName())'"
            Write-Host "123 SomeArg1='$SomeArg1', SomeArg2='$SomeArg2'"
            "some output"
        },
        @("SomeArg1Value", "SomeArg2Value"),
        @("localhost")
    )
} finally {
    $remoteHostInteractor.CloseConnections()
}
