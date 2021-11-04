function rename {
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$from,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$to,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$files,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$dir
    )

    Get-ChildItem -Path $dir -Include $files -Recurse | ForEach-Object {
        $oldName = $_.Name
        $newName = $oldName.Replace($from, $to)
        if ($oldName -ne $newName) {
            $oldPath = $_.FullName
            $fileDir = Split-Path -Path $oldPath -Parent
            $newPath = Join-Path $fileDir $newName

            Write-Host "Renaming from '$oldPath' to '$newPath'"
            Move-Item -Path $oldPath -Destination $newPath -Force
        }
    }
}

Set-PSReadLineOption -PredictionViewStyle ListView
Set-PSReadLineOption -PredictionSource History