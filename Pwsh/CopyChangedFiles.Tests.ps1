[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseDeclaredVarsMoreThanAssignments", "")]
param()
$global:ProgressPreference = "SilentlyContinue"

Describe "CopyChangedFiles" {
    BeforeAll {
        function CopyItem([string]$path, [string]$to) {
            New-Item -ItemType Directory -Path $to
            Copy-Item -Path $path -Destination $to -Force -Recurse
        }

        $copyChangedFilesScriptPath = "$PSScriptRoot\CopyChangedFiles.ps1"
    }

    BeforeEach {
        $sampleGitRepoDir = "$TestDrive\SampleGitRepoDir"
        $targetDir = "$TestDrive\tobechanged"
        $params = @{
            FromRev = "333333333333"
            ToRev = "4444444444444"
            RepoPathPart = "tobechanged"
            FileNameMask = "*"
            TargetDir = $targetDir
            RepoDir = $sampleGitRepoDir
        }
        CopyItem "$PSScriptRoot\TestData\sample_git_repo.zip" $sampleGitRepoDir
        #sample_git_repo.zip has to be created with Compress-Archive commad, otherwise localization issues are expected
        #Compress-Archive -Path ".\*" -DestinationPath "sample_git_repo.zip"
        Expand-Archive -Path "$sampleGitRepoDir\sample_git_repo.zip" -DestinationPath $sampleGitRepoDir
    }

    Context "Ctx" {
        It "Copies only changed files" {
            $params["RepoPathPart"] = "SomeDir\SomeSubdir"
            $params["TargetDir"] = $targetDir = "$TestDrive\Output\SomeDir\SomeSubdir"
            & $copyChangedFilesScriptPath @params
            Test-Path -Path "$targetDir\SomeSubSubDir\changedfile1" -PathType Leaf | Should -BeExactly $true
            (Get-ChildItem -Path "$targetDir\SomeSubSubDir" | Measure-Object).Count | Should -BeExactly 1
        }
    }

    Context "Ctx" {
        It "Copies only filtered files" {
            $params["RepoPathPart"] = "SomeDir\SomeSubdir"
            $params["FileNameMask"] = "*.txt"
            $params["TargetDir"] = $targetDir = "$TestDrive\Output"
            & $copyChangedFilesScriptPath @params
            Test-Path -Path "$targetDir\somechagnedtxtfile.txt" -PathType Leaf | Should -BeExactly $true
            Test-Path -Path "$targetDir\somechagnednontxtfile.csv" -PathType Leaf | Should -BeExactly $false
        }
    }

    Context "Ctx" {
        It "Throws on invalid rev parameter" {
            $params["FromRev"] = "origin/some-non-existing-branch"
            { & $copyChangedFilesScriptPath @params } | Should -Throw "Failed to parse revision 'origin/some-non-existing-branch'"
            $params["FromRev"] = "123456"
            { & $copyChangedFilesScriptPath @params } | Should -Throw "Hash doesn't exist in repository '123456'"
        }
    }
}