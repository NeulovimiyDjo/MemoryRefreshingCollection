using module "..\Classes\FilesHelper.psm1"
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseDeclaredVarsMoreThanAssignments", "")]
param()

Describe "FilesHelper" {
    BeforeAll {
        $filesHelper = [FilesHelper]::new()
    }

    Context "Ctx" {
        It "EnsureExists throws when path doesn't exist and does nothing otherwise" {
            $folder1 = "$TestDrive\folder1"
            { $filesHelper.EnsureExists($folder1) *>&1 } | Should -Throw "Path '*\folder1' doesn't exist"
            { $filesHelper.EnsureExists("$folder1\file1") *>&1 } | Should -Throw "Path '*\folder1\file1' doesn't exist"

            New-Item -Path $folder1 -ItemType Directory
            Out-File -FilePath "$folder1\file1" -InputObject "some content"

            { $filesHelper.EnsureExists($folder1) *>&1 } | Should -Not -Throw
            { $filesHelper.EnsureExists("$folder1\file1") *>&1 } | Should -Not -Throw
        }
    }

    Context "Ctx" {
        It "CreateDirIfNotExists creates directory and doesn't throw if it already exists" {
            $folder1 = "$TestDrive\folder1"
            $filesHelper.CreateDirIfNotExists($folder1) *>&1
            Test-Path -Path $folder1 -PathType Container | Should -BeExactly $true
            { $filesHelper.CreateDirIfNotExists($folder1) *>&1 } | Should -Not -Throw
        }
    }

    Context "Ctx" {
        It "DeleteIfExists deletes file or directory and doesn't throw if it doesn't exists" {
            $folder1 = "$TestDrive\folder1"

            New-Item -Path $folder1 -ItemType Directory
            Out-File -FilePath "$folder1\file1" -InputObject "some content"

            $filesHelper.DeleteIfExists("$folder1\file1") *>&1
            Test-Path -Path "$folder1\file1" -PathType Leaf | Should -BeExactly $false
            { $filesHelper.DeleteIfExists("$folder1\file1") *>&1 } | Should -Not -Throw

            $filesHelper.DeleteIfExists($folder1) *>&1
            Test-Path -Path $folder1 -PathType Container | Should -BeExactly $false
            { $filesHelper.DeleteIfExists($folder1) *>&1 } | Should -Not -Throw
        }
    }

    Context "Ctx" {
        It "DeleteIfExists deletes by mask and doesn't throw if it doesn't exists" {
            $folder1 = "$TestDrive\folder1"

            New-Item -Path $folder1 -ItemType Directory
            Out-File -FilePath "$folder1\file1" -InputObject "some content"
            Out-File -FilePath "$folder1\x-file1" -InputObject "some content"
            New-Item -Path "$folder1\subdir1" -ItemType Directory
            New-Item -Path "$folder1\x-subdir1" -ItemType Directory
            Out-File -FilePath "$folder1\subdir1\file1" -InputObject "some content"
            Out-File -FilePath "$folder1\x-subdir1\file1" -InputObject "some content"

            $filesHelper.DeleteIfExists("$folder1\x-*") *>&1
            Test-Path -Path "$folder1\file1" -PathType Leaf | Should -BeExactly $true
            Test-Path -Path "$folder1\x-file1" -PathType Leaf | Should -BeExactly $false
            Test-Path -Path "$folder1\subdir1" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder1\x-subdir1" -PathType Container | Should -BeExactly $false
            { $filesHelper.DeleteIfExists("$folder1\x-*") *>&1 } | Should -Not -Throw
        }
    }

    Context "Ctx" {
        It "CopyToDir throws if path doesn't exist" {
            $folder1 = "$TestDrive\folder1"
            $folder2 = "$TestDrive\folder2"
            { $filesHelper.CopyToDir("$folder1\*", $folder2) *>&1 } | Should -Throw "Path '*\folder1\*' not found during copy"
        }
    }

    Context "Ctx" {
        It "CopyToDir 'folder\*' creates needed directories and copies all files recursively" {
            $folder1 = "$TestDrive\folder1"
            New-Item -Path "$folder1\subfolder1" -ItemType Directory
            Out-File -FilePath "$folder1\subfolder1\file1" -InputObject "some content"

            $folder2 = "$TestDrive\folder2"
            $filesHelper.CopyToDir("$folder1\*", $folder2) *>&1

            Test-Path -Path "$folder2" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\subfolder1" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\subfolder1\file1" -PathType Leaf | Should -BeExactly $true
        }
    }

    Context "Ctx" {
        It "CopyToDir 'folder' creates needed directories and copies all files recursively" {
            $folder1 = "$TestDrive\folder1"
            New-Item -Path "$folder1\subfolder1" -ItemType Directory
            Out-File -FilePath "$folder1\subfolder1\file1" -InputObject "some content"

            $folder2 = "$TestDrive\folder2"
            $filesHelper.CopyToDir("$folder1", $folder2) *>&1

            Test-Path -Path "$folder2" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\folder1" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\folder1\subfolder1" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\folder1\subfolder1\file1" -PathType Leaf | Should -BeExactly $true
        }
    }

    Context "Ctx" {
        It "CopyFileToPath copies file to correct location" {
            $folder1 = "$TestDrive\folder1"
            New-Item -Path "$folder1\subfolder1" -ItemType Directory
            Out-File -FilePath "$folder1\subfolder1\file1" -InputObject "some content"

            $folder2 = "$TestDrive\folder2"
            $filesHelper.CopyFileToPath("$folder1\subfolder1\file1", "$folder2\newsubfolder1\newfilename1") *>&1

            Test-Path -Path "$folder2" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\newsubfolder1" -PathType Container | Should -BeExactly $true
            Test-Path -Path "$folder2\newsubfolder1\newfilename1" -PathType Leaf | Should -BeExactly $true
        }
    }

    Context "Ctx" {
        It "CopyFileToPath throws if target path is an existing directory" {
            $folder1 = "$TestDrive\folder1"
            New-Item -Path "$folder1\subfolder1" -ItemType Directory
            Out-File -FilePath "$folder1\subfolder1\file1" -InputObject "some content"

            $folderWithSameNameAsCopyFileTargetPath = "$TestDrive\folder2\newsubfolder1\newfilename1"
            New-Item -Path $folderWithSameNameAsCopyFileTargetPath -ItemType Directory
            { $filesHelper.CopyFileToPath("$folder1\subfolder1\file1", $folderWithSameNameAsCopyFileTargetPath) *>&1 } |
                Should -Throw "Copy file target path '*\folder2\newsubfolder1\newfilename1' already exists and it's a directory"
        }
    }
}