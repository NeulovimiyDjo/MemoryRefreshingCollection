$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function CreatePackageMetadata() {
    $dateTimeStamp = [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss")

    $manifest = New-Object System.Xml.XmlDocument
    $packageElem = $manifest.CreateElement($null, "Package", $null)
    $packageElem.SetAttribute("name", "my package name")
    $null = $manifest.AppendChild($packageElem)
    $null = $packageElem.AppendChild($manifest.CreateElement($null, "Version", $null)).AppendChild($manifest.CreateTextNode("1.0.0.0"))
    $null = $packageElem.AppendChild($manifest.CreateElement($null, "DateTimeStamp", $null)).AppendChild($manifest.CreateTextNode($dateTimeStamp))
    $manifest.Save("$PSScriptRoot\manifest.xml")

    Push-Location $PSScriptRoot
    Get-ChildItem -Path $PSScriptRoot -File -Recurse | Resolve-Path -Relative | Out-File -FilePath"$PSScriptRoot\files_list.txt"
    Pop-Location
}