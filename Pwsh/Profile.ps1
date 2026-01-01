# rename -from "fedora" -to "my_fedora" -files "fedor*.raw*" -dir "."
function rename {
    [CmdletBinding(PositionalBinding=$false)]
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

function get-file-extension-without-dot {
    [CmdletBinding(PositionalBinding=$true)]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$path
    )
    
    $extWithDot = [System.IO.Path]::GetExtension("$path")
    return $extWithDot.Substring(1, $extWithDot.Length - 1)
}

function vdsnap-info {
    [CmdletBinding(PositionalBinding=$true)]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$head
    )
    
    Write-Host "vdsnap:: Printing virtual disk info: head='$head'"
    sudo qemu-img info --backing-chain "$head"
}

function vdsnap-create {
    [CmdletBinding(PositionalBinding=$false)]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$head,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$backing
    )

    $hFormat = get-file-extension-without-dot -path "$head"
    $bFormat = get-file-extension-without-dot -path "$backing"

    Write-Host "vdsnap:: Creating virtual disk: head='$head' backing='$backing'"
    sudo qemu-img create -o compression_type=zstd -f $hFormat -F $bFormat -b "$backing" "$head"
}

# This will only change backing file, all up-chain files will stay untouched BUT will become USELESS with the changed base.
function vdsnap-commit {
    [CmdletBinding(PositionalBinding=$false)]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$head,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$backing
    )
    
    Write-Host "vdsnap:: Committing virtual disk: head='$head' backing='$backing'"
    sudo qemu-img commit -d -b "$backing" "$head"
}

# This will only change head file, all down-chain files will stay untouched.
function vdsnap-rebase {
    [CmdletBinding(PositionalBinding=$false)]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$head,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$backing
    )
    
    $hFormat = get-file-extension-without-dot -path "$head"
    $bFormat = get-file-extension-without-dot -path "$backing"

    Write-Host "vdsnap:: Rebasing virtual disk: head='$head' backing='$backing'"
    sudo qemu-img rebase -f $hFormat -F $bFormat -b "$backing" "$head"
}

Set-PSReadLineOption -PredictionViewStyle ListView
Set-PSReadLineOption -PredictionSource History
