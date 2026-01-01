param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Revision,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$NexusAddressMaven,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$NexusUser,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$NexusPassword
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function ParseRevAndThrowOnError {
    param([string]$rev)
    $revCommitHash = ((& git rev-parse --verify --quiet --short=12 $rev) | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to parse revision '$rev'"
    }
    (& git cat-file -e $revCommitHash) | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Hash doesn't exist in repository '$revCommitHash'"
    }
    return $revCommitHash
}

function CheckoutRev([string]$rev) {
    Write-Host "Checking out sources revision '$rev' to separate folder"
    bash -c "mkdir ./repo_files_to_zip/"
    & git --work-tree="$(pwd)/repo_files_to_zip" checkout $rev -- .
}

function ZipRepo([string]$version) {
    Write-Host "Zipping source code for '$version'"
    [System.IO.Compression.ZipFile]::CreateFromDirectory("./repo_files_to_zip", "./source_code-$version.zip", [System.IO.Compression.CompressionLevel]::Fastest, $false, [System.Text.Encoding]::UTF8)
}

function CurlUploadToMaven([string]$artifactId, [string]$version, [string]$mavenUrl, [string]$user, [string]$pass) {
    Write-Host "Uploading '$artifactId' '$version' to maven '$mavenUrl'"
    curl $mavenUrl -LfSs `
        --cacert "$PSScriptRoot/ca-bundle.crt" `
        --user "${user}:${pass}" `
        --request POST `
        -F "maven2.groupId=my_group" `
        -F "maven2.artifactId=$artifactId" `
        -F "maven2.version=$version" `
        -F "maven2.packaging=zip" `
        -F "maven2.asset1=@$artifactId-$version.zip" `
        -F "maven2.asset1.extension=zip" `
        -F "maven2.generate-pom=false"
    if ($LASTEXITCODE -ne 0) { throw "Curl upload to maven failed" }
}

$sourcesRev = ParseRevAndThrowOnError $Revision
$versionStr = "$sourcesRev-$Version"

CheckoutRev $sourcesRev
ZipRepo $versionStr
$sha256 = $(sha256sum "./source_code-$versionStr.zip" | cut -f 1 -d " ")
CurlUploadToMaven "source_code" $versionStr $NexusAddressMaven $NexusUser $NexusPassword
