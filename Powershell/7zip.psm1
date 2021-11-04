Import-Module $PSScriptRoot\Messages
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

function Zip([string]$7zPath, [string]$sourceDir, [string]$outputFile) {
	if (Test-Path $7zPath) {
		$buildFiles = [string]::Format("{0}\*", $sourceDir)
		$res = & $7zPath a -tzip -ad -slp -sae -sdel $outputFile $buildFiles
	} else {
		Error "7z not found"
		throw
	}
}

function ZipTo7zHighCompression([string]$7zPath, [string]$sourceDir, [string]$outputFile) {
	if (Test-Path $7zPath) {
		$buildFiles = [string]::Format("{0}\*", $sourceDir)
		& $7zPath a -t7z -m0=lzma -mx=9 -mfb=256 -md=256m -ms=on -mmt=off -ad -slp -sae -sdel $outputFile $buildFiles
	} else {
		Error "7z not found"
		throw
	}
}

function Unzip([string]$7zPath, [string]$path, [string]$outputDir) {
	if (Test-Path $7zPath) {
		$archivePath = [string]([System.IO.Path]::GetDirectoryName($path))
		$res = & $7zPath x -slp -aou $path -o"$outputDir"
	} else {
		Error "7z not found"
		throw
	}
}


Export-ModuleMember -Function Zip
Export-ModuleMember -Function ZipTo7zHighCompression
Export-ModuleMember -Function Unzip
