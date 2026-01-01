using module ".\Translit.psm1"

Describe "Translit" {
    Context "Ctx" {
        It "CheckNoCyrillicFileNames throws on bad file name" {
            Out-File -FilePath "$TestDrive\русское имя файла" -InputObject "some content"
            { CheckNoCyrillicFileNames $TestDrive } | Should -Throw "File name contains cyrillic symbols: '*\русское имя файла'"
        }
    }

    Context "Ctx" {
        It "CheckNoCyrillicFileNames does not throw on good file name" {
            Out-File -FilePath "$TestDrive\english_name_.txt" -InputObject "some content"
            { CheckNoCyrillicFileNames $TestDrive } | Should -Not -Throw
        }
    }

    Context "Ctx" {
        It "TransliterateCyrillicFileNames replaces bad file name to good according to translit chart" {
            Out-File -FilePath "$TestDrive\русское имя файла" -InputObject "some content"
            TransliterateCyrillicFileNames $TestDrive
            Get-ChildItem -Path $TestDrive |
                Select-Object -First 1 -ExpandProperty FullName |
                Should -BeExactly "$TestDrive\russkoe_imya_fajla"
        }
    }
}