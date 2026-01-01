$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$sha256 = $(sha256sum "1.txt" | cut -f 1 -d " ")
$files = @()
$files += [ordered]@{
    path = "1.txt"
    checksum = "sha256:$sha256"
}
$revInfo = [ordered]@{
    CommitHash = ((git rev-parse --short HEAD) | Out-String).Trim()
    Files = $files
}
$revInfo | ConvertTo-Json -Depth 4 | Out-File -FilePath "rev-info.json" -Encoding utf8
