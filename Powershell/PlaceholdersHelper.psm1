using module ".\Logger.psm1"
using module ".\FilesHelper.psm1"
using module ".\Encryptor.psm1"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

class PlaceholderPatterns {
    static [string]$Simple = "{{(.+?)}}"
    static [string]$Super = "{{{(.+?)}}}"
}

enum EscapeRules {
    None = -1
    Csv = 1
}

class PlaceholdersHelper {
    [ValidateNotNull()][Logger]$Logger
    [ValidateNotNull()][FilesHelper]$FilesHelper
    [ValidateNotNull()][Encryptor]$Encryptor
    hidden [System.Collections.Generic.HashSet[string]]$CurrentReplacementUnknownPlaceholders = @()
    hidden [bool]$CurrentReplacementChangedSomething

    PlaceholdersHelper() {
        $this.Logger = New-Object Logger Trace
        $this.FilesHelper = New-Object FilesHelper $this.Logger
        $this.Encryptor = New-Object Encryptor $this.Logger
    }

    [hashtable]CreatePlaceholdersDictFromCsvString([string]$placeholdersCsvString) {
        $this.Logger.Trace("Creating placeholders dictionary from csv string")
        $csvRowsList = ConvertFrom-Csv -InputObject $placeholdersCsvString -Delimiter ";"
        return $this.ConvertCsvRowsListToDict($csvRowsList)
    }

    [hashtable]CreatePlaceholdersDictFromFile([string]$placeholdersFile) {
        $this.Logger.Trace("Creating placeholders dictionary from file '$placeholdersFile'")
        $csvRowsList = Import-Csv -Path $placeholdersFile -Delimiter ";" -Encoding utf8
        return $this.ConvertCsvRowsListToDict($csvRowsList)
    }

    hidden [hashtable]ConvertCsvRowsListToDict([PSCustomObject[]]$csvRowsList) {
        $placeholdersDict = @{}
        foreach ($csvRow in $csvRowsList) {
            $placeholdersDict[$csvRow.Placeholder] = $csvRow
        }
        return $placeholdersDict
    }

    [string]CreateCsvStringFromPlaceholdersDict([hashtable]$placeholdersDict) {
        $this.Logger.Trace("Creating csv string from placeholders dictionary")
        $lines = $this.ConvertPlaceholdersDictToCsvTextRows($placeholdersDict)
        return $lines -Join "`n"
    }

    [void]SavePlaceholdersDictToFile([hashtable]$placeholdersDict, [string]$placeholdersFile) {
        $this.Logger.Trace("Saving placeholders dictionary to file '$placeholdersFile'")
        $this.FilesHelper.CreateParentDirIfNotExists($placeholdersFile)
        $this.ConvertPlaceholdersDictToCsvTextRows($placeholdersDict) |
            Out-File -FilePath $placeholdersFile -Force -Encoding utf8
    }

    hidden [string[]]ConvertPlaceholdersDictToCsvTextRows([hashtable]$placeholdersDict) {
        return $placeholdersDict.Values |
            Sort-Object -Property Placeholder |
            ConvertTo-Csv -Delimiter ";" -NoTypeInformation
    }

    [hashtable]CreatePlaceholdersDictFromEncryptedCsvString([string]$placeholdersEncryptedCsvString, [string]$encryptedPlaceholdersPassword) {
        $this.Logger.Trace("Creating placeholders dictionary from encrypted csv string")
        $csvRowsList = ConvertFrom-Csv -InputObject $placeholdersEncryptedCsvString -Delimiter ";"
        return $this.ConvertEncryptedCsvRowsListToDict($csvRowsList, $encryptedPlaceholdersPassword)
    }

    [hashtable]CreatePlaceholdersDictFromEncryptedFile([string]$encryptedPlaceholdersFile, [string]$encryptedPlaceholdersPassword) {
        $this.Logger.Trace("Creating placeholders dictionary from encrypted file '$encryptedPlaceholdersFile'")
        $csvRowsList = Import-Csv -Path $encryptedPlaceholdersFile -Delimiter ";" -Encoding utf8
        return $this.ConvertEncryptedCsvRowsListToDict($csvRowsList, $encryptedPlaceholdersPassword)
    }

    hidden [hashtable]ConvertEncryptedCsvRowsListToDict([PSCustomObject[]]$encryptedCsvRowsList, [string]$encryptedPlaceholdersPassword) {
        $placeholdersDict = @{}
        foreach ($encryptedCsvRow in $encryptedCsvRowsList) {
            $decryptedValue = $this.Encryptor.DecryptString($encryptedCsvRow.Value, $encryptedPlaceholdersPassword)
            $placeholdersDict[$encryptedCsvRow.Placeholder] = [PSCustomObject]@{
                Placeholder = $encryptedCsvRow.Placeholder
                Value = $decryptedValue
                Description = $encryptedCsvRow.Description
                ValueChanged = $true #unless explicitly set to false to avoid line changes due to different reencryption on save
                OldEncryptedValue = $encryptedCsvRow.Value
            }
        }
        return $placeholdersDict
    }

    [string]CreateEncryptedCsvStringFromPlaceholdersDict([hashtable]$placeholdersDict, [string]$encryptedPlaceholdersPassword) {
        $this.Logger.Trace("Creating encrypted csv string from placeholders dictionary")
        $lines = $this.ConvertPlaceholdersDictToEncryptedCsvTextRows($placeholdersDict, $encryptedPlaceholdersPassword)
        return $lines -Join "`n"
    }

    [void]SavePlaceholdersDictToEncryptedFile([hashtable]$placeholdersDict, [string]$encryptedPlaceholdersFile, [string]$encryptedPlaceholdersPassword) {
        $this.Logger.Trace("Saving placeholders dictionary to encrypted file '$encryptedPlaceholdersFile'")
        $this.FilesHelper.CreateParentDirIfNotExists($encryptedPlaceholdersFile)
        $this.ConvertPlaceholdersDictToEncryptedCsvTextRows($placeholdersDict, $encryptedPlaceholdersPassword) |
            Out-File -FilePath $encryptedPlaceholdersFile -Force -Encoding utf8
    }

    hidden [string[]]ConvertPlaceholdersDictToEncryptedCsvTextRows([hashtable]$placeholdersDict, [string]$encryptedPlaceholdersPassword) {
        return $placeholdersDict.Values |
            ForEach-Object {
                $valueChanged = $true
                if ($_.PSObject.Properties.Name -contains "ValueChanged") {
                    $valueChanged = $_.ValueChanged
                }

                $encryptedValue = $null
                if ($valueChanged) {
                    $encryptedValue = $this.Encryptor.EncryptString($_.Value, $encryptedPlaceholdersPassword)
                } else {
                    $encryptedValue = $_.OldEncryptedValue
                }

                [PSCustomObject]@{
                    Placeholder = $_.Placeholder
                    Value = $encryptedValue
                    Description = $_.Description
                }
            } |
            Sort-Object -Property Placeholder |
            ConvertTo-Csv -Delimiter ";" -NoTypeInformation
    }

    [void]ReplacePlaceholdersInDir([string]$dir, [hashtable]$placeholdersDict, [string]$pattern, [string[]]$filesWithPlaceholdersExtensions) {
        $this.Logger.Trace("Replacing placeholders in directory '$dir'")
        $filePathsList = Get-ChildItem -Path $dir -Recurse -Include $filesWithPlaceholdersExtensions | Select-Object -ExpandProperty FullName
        $this.CurrentReplacementUnknownPlaceholders.Clear()
        foreach ($filePath in $filePathsList) {
            if ($filePath.EndsWith(".xml") -and ((Get-Item -Path $filePath).Length / 1kb -gt 500)) {
                continue # Skip big xml files which are documentations for dotnet dlls
            }
            $this.Logger.Trace("Replacing placeholders in file '$filePath'")
            $this.ReplacePlaceholdersInFileImpl($filePath, $placeholdersDict, $pattern, [EscapeRules]::None)
        }
        $this.LogUnknownPlaceholdersAndThrowIfAny()
    }

    [void]ReplacePlaceholdersInFile([string]$filePath, [hashtable]$placeholdersDict, [string]$pattern, [EscapeRules]$escapeRules) {
        $this.Logger.Trace("Replacing placeholders in file '$filePath'")
        $this.CurrentReplacementUnknownPlaceholders.Clear()
        $this.ReplacePlaceholdersInFileImpl($filePath, $placeholdersDict, $pattern, $escapeRules)
        $this.LogUnknownPlaceholdersAndThrowIfAny()
    }

    [string]ReplacePlaceholdersInString([string]$stringWithPlaceholders, [hashtable]$placeholdersDict, [string]$pattern, [EscapeRules]$escapeRules) {
        $this.Logger.Trace("Replacing placeholders in string")
        $this.CurrentReplacementUnknownPlaceholders.Clear()
        $stringWithReplacedPlaceholders = $this.ReplacePlaceholdersInStringImpl($stringWithPlaceholders, $placeholdersDict, $pattern, $escapeRules)
        $this.LogUnknownPlaceholdersAndThrowIfAny()
        return $stringWithReplacedPlaceholders
    }

    hidden [void]ReplacePlaceholdersInFileImpl([string]$filePath, [hashtable]$placeholdersDict, [string]$pattern, [EscapeRules]$escapeRules) {
        $fileContent = Get-Content -Path $filePath -Encoding utf8 -Raw
        $this.CurrentReplacementChangedSomething = $false
        $replacedFileContent = $this.ReplacePlaceholdersInStringImpl($fileContent, $placeholdersDict, $pattern, $escapeRules)
        if ($this.CurrentReplacementChangedSomething) {
            $this.Logger.Trace("Resaving file '$filePath' with replaced placeholders")
            $replacedFileContent | Out-File -FilePath $filePath -Encoding utf8
        }
    }

    hidden [string]ReplacePlaceholdersInStringImpl([string]$stringWithPlaceholders, [hashtable]$placeholdersDict, [string]$pattern, [EscapeRules]$escapeRules) {
        $sb = New-Object System.Text.StringBuilder $stringWithPlaceholders
        $stringWithPlaceholders | Select-String -AllMatches $pattern | ForEach-Object {
            $placeholderKeysInFile = $_.Matches | Select-Object -ExpandProperty Value
            foreach ($placeholderKey in $placeholderKeysInFile) {
                if (-not $placeholdersDict.ContainsKey($placeholderKey)) {
                    $this.Logger.Error("Unknown placeholder '$placeholderKey'")
                    $this.CurrentReplacementUnknownPlaceholders.Add($placeholderKey)
                } else {
                    $placeholderValue = "$($placeholdersDict[$placeholderKey].Value)"
                    $quotedPlaceholderValue = $this.EscapeValue($placeholderValue, $escapeRules)
					$sb.Replace($placeholderKey, $quotedPlaceholderValue)
                    $this.CurrentReplacementChangedSomething = $true
                    $this.Logger.Trace("Successfully replaced placeholder '$placeholderKey'")
                }
            }
        }
        return $sb.ToString()
    }
 
    hidden [string]EscapeValue([string]$value, [EscapeRules]$escapeRules) {
        if ($escapeRules -eq [EscapeRules]::None) {
            return $value
        } elseif ($escapeRules -eq [EscapeRules]::Csv) {
            return $value.Replace('"', '""')
        } else {
            throw "Invalid escape rule"
        }
    }

    hidden [void]LogUnknownPlaceholdersAndThrowIfAny() {
        if ($this.CurrentReplacementUnknownPlaceholders.Count -gt 0) {
            $this.Logger.Error(@("Unknown placeholders list:") + $this.CurrentReplacementUnknownPlaceholders)
            throw "Unknown placeholders were found"
        }
    }
}
