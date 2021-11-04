using module "..\Classes\Encryptor.psm1"
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseDeclaredVarsMoreThanAssignments", "")]
param()

Describe "Encryptor" {
    BeforeAll {
        $encryptor = [Encryptor]::new()
        $encryptor.Logger.Verbosity = "None"
    }

    It "EncryptDecrypt works" {
        $password = "some password"
        $dataString = "test data bla bla"
        $encryptedString = $encryptor.EncryptString($dataString, $password)
        $decryptedString = $encryptor.DecryptString($encryptedString, $password)
        $decryptedString | Should -BeExactly $dataString
    }

    It "EncryptDecrypt throws on wrong password" {
        $password = "some password"
        $dataString = "test data bla bla"
        $encryptedString = $encryptor.EncryptString($dataString, $password)
        { $null = $encryptor.DecryptString($encryptedString, "other password") } | Should -Throw "Decrypted data hash check failed"
    }
}