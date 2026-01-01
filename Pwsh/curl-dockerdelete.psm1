$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function CurlDownload([string]$downloadUrl, [string]$outFile, [string]$user, [string]$pass) {
    Write-Host "Downloading from '$downloadUrl' to '$outFile'"
    curl $downloadUrl -LfSs -o $outFile `
        --cacert "$PSScriptRoot/trusted-ca-bundle.crt" `
        --user "${user}:${pass}"
    if ($LASTEXITCODE -ne 0) { throw "Curl download failed" }
}

function DockerDelete([string]$imageName, [string]$exceptTag = $null) {
    Write-Host "Deleting $imageName (exceptTag='$exceptTag')"

    $exceptHashes = New-Object System.Collections.Generic.List[System.String]
    $images = docker image ls |
        ForEach-Object {
            $attrArray = $_.Trim() -split "\s+"
            $name = $attrArray[0]
            $tag = $attrArray[1]
            $hash = $attrArray[2]
            if ($name -match "^.*$([Regex]::Escape($imageName))$") {
                Write-Host "Found ${name}:$tag"
                if ($tag -match "^$([Regex]::Escape($exceptTag))$") {
                    Write-Host "Skipping as exception ${name}:$tag"
                    $exceptHashes.Add($hash)
                } else {
                    $hash
                }
            }
        } |
        Select-Object -Unique

    $images = $images | Where-Object { $_ -notin $exceptHashes }
    if (($images | Measure-Object).Count -gt 0) {
        docker rmi --force $images
    }
}
