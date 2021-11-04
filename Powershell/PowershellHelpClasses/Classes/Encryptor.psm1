using module "..\Classes\Logger.psm1"
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "")]
param()
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function ExecuteWithDispose([ScriptBlock]$scriptBlock, $disposable) {
    try {
        & $scriptBlock
    } finally {
        try {
            $disposable.Dispose()
        } catch {
            if (-not $_.Exception.Message.Contains("Padding is invalid and cannot be removed.")) {
                throw
            }
        }
    }
}
Export-ModuleMember #none

class Encryptor {
    [int]$SaltLengthInBytes = 16
    [int]$KeyLengthInBytes = 32
    [ValidateNotNull()][Logger]$Logger

    Encryptor() {
        $this.Init((New-Object Logger([Verbosity]::Trace)))
    }
    Encryptor([Logger]$logger) {
        $this.Init($logger)
    }
    hidden [void]Init([Logger]$logger) {
        $this.Logger = $logger
    }

    [string]EncryptString([string]$dataString, [string]$password) {
        $this.Logger.Trace("Encrypting string")

        $data = [Text.Encoding]::UTF8.GetBytes($dataString)
        $dataWithPrependedDataHash = [Encryptor]::GetDataWithPrependedDataHash($data)
        $salt = $this.GenerateSalt()
        $key = New-Object Security.Cryptography.Rfc2898DeriveBytes($password, $salt)

        $encryptedString = $null
        $aes = [Security.Cryptography.Aes]::Create()
        ExecuteWithDispose {
            $aes.Key = $key.GetBytes($this.KeyLengthInBytes)
            $aes.GenerateIV()

            $encryptor = $aes.CreateEncryptor()
            ExecuteWithDispose {
                $ms = New-Object IO.MemoryStream
                ExecuteWithDispose {
                    $cs = New-Object Security.Cryptography.CryptoStream($ms, $encryptor, "Write")
                    ExecuteWithDispose {
                        $bw = New-Object IO.BinaryWriter($cs)
                        ExecuteWithDispose {
                            $bw.Write($dataWithPrependedDataHash)
                        } $bw
                    } $cs

                    ([ref]$encryptedString).Value = @(
                        [Convert]::ToBase64String($aes.IV)
                        [Convert]::ToBase64String($salt)
                        [Convert]::ToBase64String($ms.ToArray())
                    ) -Join ":"
                } $ms
            } $encryptor
        } $aes
        return $encryptedString
    }

    [string]DecryptString([string]$encryptedString, [string]$password) {
        $this.Logger.Trace("Decrypting string")

        $iv_salt_encryptedBytes = $encryptedString.Split(@(":"), [System.StringSplitOptions]::new())
        [byte[]]$iv = [Convert]::FromBase64String($iv_salt_encryptedBytes[0])
        [byte[]]$salt = [Convert]::FromBase64String($iv_salt_encryptedBytes[1])
        [byte[]]$encryptedBytes = [Convert]::FromBase64String($iv_salt_encryptedBytes[2])
        $key = New-Object Security.Cryptography.Rfc2898DeriveBytes($password, $salt)

        $decryptedString = $null
        $aes = [Security.Cryptography.Aes]::Create()
        ExecuteWithDispose {
            $aes.Key = $key.GetBytes($this.KeyLengthInBytes)
            $aes.IV = $iv

            $decryptor = $aes.CreateDecryptor()
            ExecuteWithDispose {
                $ms = New-Object IO.MemoryStream
                ExecuteWithDispose {
                    $cs = New-Object Security.Cryptography.CryptoStream($ms, $decryptor, "Write")
                    ExecuteWithDispose {
                        $bw = New-Object IO.BinaryWriter($cs)
                        ExecuteWithDispose {
                            $bw.Write($encryptedBytes)
                        } $bw
                    } $cs

                    $dataWithPrependedDataHash = $ms.ToArray()
                    $data = [Encryptor]::GetDataOrThrowOnFailedPrependedDataHashCheck($dataWithPrependedDataHash)
                    ([ref]$decryptedString).Value = [Text.Encoding]::UTF8.GetString($data)
                }$ms
            } $decryptor
        } $aes
        return $decryptedString
    }

    hidden [byte[]]GenerateSalt() {
        $salt = [byte[]]::new($this.SaltLengthInBytes)
        $rngProvider = New-Object Security.Cryptography.RNGCryptoServiceProvider
        ExecuteWithDispose {
            $rngProvider.GetBytes($salt)
        } $rngProvider
        return $salt
    }

    hidden static [byte[]]GetDataWithPrependedDataHash([byte[]] $data) {
        $sha256 = [System.Security.Cryptography.SHA256]::Create()
        $dataHash = $sha256.ComputeHash($data)
        $dataWithPrependedDataHash = $dataHash + $data
        return $dataWithPrependedDataHash
    }

    hidden static [byte[]]GetDataOrThrowOnFailedPrependedDataHashCheck([byte[]] $dataWithPrependedDataHash) {
        $hashLength = 256 / 8
        $prependedDataHash = $dataWithPrependedDataHash[0 .. ($hashLength - 1)]
        $data = $dataWithPrependedDataHash[$hashLength .. ($dataWithPrependedDataHash.Length - 1)]
        $sha256 = [System.Security.Cryptography.SHA256]::Create()
        $actualDataHash = $sha256.ComputeHash($data)
        if ((Compare-Object -ReferenceObject $prependedDataHash -DifferenceObject $actualDataHash | Measure-Object).Count -ne 0) {
            throw "Decrypted data hash check failed"
        }
        return $data
    }
}
