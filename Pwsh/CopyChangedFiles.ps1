[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$FromRev,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ToRev,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$RepoPathPart,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$FileNameMask,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$TargetDir,
    [Parameter(Mandatory = $false)][ValidateNotNullOrEmpty()][string]$RepoDir = "$PSScriptRoot\..\.."
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function ParseRevAndThrowOnError {
    param([string]$rev)
    $revCommitHash = ((& git rev-parse --verify --quiet $rev) | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to parse revision '$rev'"
    }
    (& git cat-file -e $revCommitHash) | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Hash doesn't exist in repository '$revCommitHash'"
    }
    return $revCommitHash
}

Push-Location $RepoDir
try {
    & git config --local core.quotepath off
    $oldEncoding = [Console]::OutputEncoding
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $fromRevCommitHash = ParseRevAndThrowOnError $FromRev
    $toRevCommitHash = ParseRevAndThrowOnError $ToRev
    [Array]$filePathsRelativeToRepoRoot = & git diff --diff-filter=ACMRT --name-only $fromRevCommitHash $toRevCommitHash -- "$RepoPathPart\$FileNameMask"
    [Console]::OutputEncoding = $oldEncoding
    & git config --local --unset core.quotepath

    if ($null -ne $filePathsRelativeToRepoRoot) {
        foreach ($filePath in $filePathsRelativeToRepoRoot) {
            $filePathRelativeToRepoPathPart = $filePath.SubString($RepoPathPart.Length)
            $targetFilePath = Join-Path $TargetDir $filePathRelativeToRepoPathPart

            $targetFilePathDir = Split-Path -Path $targetFilePath -Parent
            If (-not (Test-Path $targetFilePathDir)) {
                New-Item -ItemType Directory -Force -Path $targetFilePathDir | Out-Null
            }

            Copy-Item -Path $filePath -Destination $targetFilePath -Recurse
        }
    }
} finally {
    Pop-Location
}
