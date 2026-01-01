function Find-Utf8WithoutBom {
    param (
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Path
    )

    $bomSignature = [byte[]](0xEF, 0xBB, 0xBF)
    $utf8NoBomFiles = @()

    Get-ChildItem -Path $Path -Recurse -File | ForEach-Object {
        $filePath = $_.FullName
        if ((
                $filePath.EndsWith(".sln") -or
                $filePath.EndsWith(".props") -or
                $filePath.EndsWith(".targets") -or
                $filePath.EndsWith(".csproj") -or
                $filePath.EndsWith(".cshtml") -or
                $filePath.EndsWith(".razor") -or
                $filePath.EndsWith(".xaml") -or
                $filePath.EndsWith(".cs")
            ) -and -not (
                $filePath.Contains("\node_modules\") -or
                $filePath.Contains("\bin\") -or
                $filePath.Contains("\obj\"))) {
            try {
                $stream = New-Object System.IO.FileStream($filePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read)
                $reader = New-Object System.IO.BinaryReader($stream)
                $header = $reader.ReadBytes(3)
                $reader.Close()
                $stream.Close()

                if (-not ([System.Linq.Enumerable]::SequenceEqual($header, $bomSignature))) {
                    # Attempt to read as UTF-8 to confirm encoding
                    try {
                        $null = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
                        $utf8NoBomFiles += $filePath
                        Write-Output "$filePath"
                    } catch [System.Text.DecoderFallbackException] {
                        # Not valid UTF-8
                    }
                }
            } catch {
                Write-Warning "Error processing $($filePath): $($_.Exception.Message)"
            }
        }
    }
}
#Find-Utf8WithoutBom -Path ./src > 1.txt
