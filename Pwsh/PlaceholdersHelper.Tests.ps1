using module ".\PlaceholdersHelper.psm1"
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseDeclaredVarsMoreThanAssignments", "")]
param()

Describe "PlaceholdersHelper" {
    BeforeAll {
        function AssertFilesAreEqual {
            param($Actual, $Expected)
            $actualContent = Get-Content -Raw -Path $Actual -Encoding UTF8
            $expectedContet = Get-Content -Raw -Path $Expected -Encoding UTF8
            $actualContent | Should -BeExactly $expectedContet
        }

        function AssertHashtablesAreEqual {
            param([hashtable]$Actual, [hashtable]$Expected, [switch]$IgnoreKeyOrder)
            if ($IgnoreKeyOrder) {
                $actualJson = ConvertTo-Json ($Actual.GetEnumerator() | Sort-Object -Property Key)
                $expectedJson = ConvertTo-Json ($Expected.GetEnumerator() | Sort-Object -Property Key)
                $actualJson | Should -BeExactly $expectedJson
            } else {
                $actualJson = ConvertTo-Json $Actual
                $expectedJson = ConvertTo-Json $Expected
                $actualJson | Should -BeExactly $expectedJson
            }
        }

        function ExcludePropertyFromAllHashtableValues([hashtable]$placeholdersDict, [string]$propertyName) {
            $newPlaceholdersDict = @{}
            foreach ($placeholderKey in $placeholdersDict.Keys) {
                $newPlaceholdersDict[$placeholderKey] = $placeholdersDict[$placeholderKey] |
                    Select-Object -Property * -ExcludeProperty $propertyName
            }
            return $newPlaceholdersDict
        }

        function CopyItem([string]$path, [string]$to) {
            New-Item -ItemType Directory -Path $to
            Copy-Item -Path $path -Destination $to -Force -Recurse
        }

        $placeholdersHelper = [PlaceholdersHelper]::new()
    }

    BeforeEach {
        $tmpConfiguration = "$TestDrive\Configuration"
        CopyItem "$PSScriptRoot\TestData\SampleConfiguration\*" $tmpConfig

        $configFile = "$tmpConfig\config.json"
        $placeholdersFile = "$tmpConfig\placeholders.csv"
        $superplaceholdersFile = "$tmpConfig\superplaceholders.csv"
        $encryptedSuperplaceholdersFile = "$tmpConfig\encrypted_superplaceholders.csv"

        $configFileContent = Get-Content -Raw -Path $configFile -Encoding UTF8
        $placeholdersFileContent = Get-Content -Raw -Path $placeholdersFile -Encoding UTF8
        $superplaceholdersFileContent = Get-Content -Raw -Path $superplaceholdersFile -Encoding UTF8
        $encryptedSuperplaceholdersFileContent = Get-Content -Raw -Path $encryptedSuperplaceholdersFile -Encoding UTF8

        $savedFile = "$TestDrive\TmpResults\SavedFile.txt"
    }

    Context "Ctx" {
        It "Simple placeholders are getting loaded and saved correctly" {
            $dictFromFile = $placeholdersHelper.CreatePlaceholdersDictFromFile($placeholdersFile)
            $dictFromString = $placeholdersHelper.CreatePlaceholdersDictFromCsvString($placeholdersFileContent)
            AssertHashtablesAreEqual -Actual $dictFromString -Expected $dictFromFile

            $placeholdersHelper.SavePlaceholdersDictToFile($dictFromFile, $savedFile)
            $dictFromSavedFile = $placeholdersHelper.CreatePlaceholdersDictFromFile($savedFile)
            AssertHashtablesAreEqual -Actual $dictFromSavedFile -Expected $dictFromFile -IgnoreKeyOrder

            $createdCsvString = $placeholdersHelper.CreateCsvStringFromPlaceholdersDict($dictFromFile).Replace("`r`n", "`n").Trim()
            $contentFromSavedFile = (Get-Content -Raw -Path $savedFile -Encoding UTF8).Replace("`r`n", "`n").Trim()
            $createdCsvString | Should -BeExactly $contentFromSavedFile
        }
    }

    Context "Ctx" {
        It "Encrypted placeholders are getting loaded and saved correctly" {
            $password = "123"
            $dictFromFile = $placeholdersHelper.CreatePlaceholdersDictFromEncryptedFile($encryptedSuperplaceholdersFile, $password)
            $dictFromString = $placeholdersHelper.CreatePlaceholdersDictFromEncryptedCsvString($encryptedSuperplaceholdersFileContent, $password)
            AssertHashtablesAreEqual -Actual $dictFromString -Expected $dictFromFile

            $placeholdersHelper.SavePlaceholdersDictToEncryptedFile($dictFromFile, $savedFile, $password)
            $dictFromSavedFile = $placeholdersHelper.CreatePlaceholdersDictFromEncryptedFile($savedFile, $password)
            $dictFromFile = ExcludePropertyFromAllHashtableValues $dictFromFile "OldEncryptedValue"
            $dictFromSavedFile = ExcludePropertyFromAllHashtableValues $dictFromSavedFile "OldEncryptedValue"
            AssertHashtablesAreEqual -Actual $dictFromSavedFile -Expected $dictFromFile -IgnoreKeyOrder

            $createdCsvString = $placeholdersHelper.CreateEncryptedCsvStringFromPlaceholdersDict($dictFromFile, $password).Replace("`r`n", "`n").Trim()
            $contentFromSavedFile = (Get-Content -Raw -Path $savedFile -Encoding UTF8).Replace("`r`n", "`n").Trim()
            $createdCsvString = $createdCsvString -replace '___";"(.+?)";"', '___";"HERE_WAS_ENCRYPTED_STRING_DIFFERENT_EACH_TIME";"'
            $contentFromSavedFile = $contentFromSavedFile -replace '___";"(.+?)";"', '___";"HERE_WAS_ENCRYPTED_STRING_DIFFERENT_EACH_TIME";"'
            $createdCsvString | Should -BeExactly $contentFromSavedFile
        }
    }

    Context "Ctx" {
        It "Encrypted placeholders don't reencrypt values if marked unchanged" {
            $password = "123"
            $dictFromFile = $placeholdersHelper.CreatePlaceholdersDictFromEncryptedFile($encryptedSuperplaceholdersFile, $password)
            $dictFromFile.Values | ForEach-Object { $_.ValueChanged = $false }
            $placeholdersHelper.SavePlaceholdersDictToEncryptedFile($dictFromFile, $savedFile, $password)
            AssertFilesAreEqual -Actual $savedFile -Expected $encryptedSuperplaceholdersFile -IgnoreKeyOrder
        }
    }

    Context "Ctx" {
        It "Placeholders get replaced" {
            $password = "123"
            $superplaceholdersDict = @{}
            $superplaceholdersDict += $placeholdersHelper.CreatePlaceholdersDictFromFile($superplaceholdersFile)
            $superplaceholdersDict += $placeholdersHelper.CreatePlaceholdersDictFromEncryptedFile($encryptedSuperplaceholdersFile, $password)

            $configWithReplacedPlaceholders = $placeholdersHelper.ReplacePlaceholdersInString(
                $configFileContent, $superplaceholdersDict, [PlaceholderPatterns]::Super, [EscapeRules]::None)
            $configWithReplacedPlaceholders | Should -BeLikeExactly '*"SomePlaceholder": "SomePlaceholderValue",*'
            $configWithReplacedPlaceholders | Should -BeLikeExactly '*"SomeSecretPlaceholder": "SomeSecretPlaceholderValue",*'

            $placeholdersHelper.ReplacePlaceholdersInFile($placeholdersFile, $superplaceholdersDict, [PlaceholderPatterns]::Super, [EscapeRules]::None)
            $placeholdersFile | Should -FileContentMatchExactly '^{{SomePlaceholder}};SomePlaceholderValue;.*'

            $placeholdersHelper.ReplacePlaceholdersInDir($tmpConfig, $superplaceholdersDict, [PlaceholderPatterns]::Super, "*.json")
            $configFile | Should -FileContentMatchExactly '.*"SomePlaceholder": "SomePlaceholderValue",.*'
            $configFile | Should -FileContentMatchExactly '.*"SomeSecretPlaceholder": "SomeSecretPlaceholderValue",.*'
        }
    }
}