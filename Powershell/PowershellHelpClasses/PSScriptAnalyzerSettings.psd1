@{
    Severity = @(
        'Error'
        'Warning'
        'Information'
    )

    IncludeRules = @(
        '*'
    )

    ExcludeRules = @(
        'PSAvoidUsingWriteHost'
    )

    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            Kind = 'space'
            IndentationSize = 4
            PipelineIndentation = "IncreaseIndentationForFirstPipeline"
        }

        PSUseConsistentWhitespace = @{
            Enable = $true
            CheckOpenBrace = $true
            CheckInnerBrace = $true
            CheckPipe = $true
            CheckPipeForRedundantWhitespace = $true
            CheckOpenParen = $true
            CheckOperator = $true
            CheckSeparator = $true
            IgnoreAssignmentOperatorInsideHashTable = $false
        }

        PSPlaceOpenBrace = @{
            Enable = $true
            OnSameLine = $true
            NewLineAfter = $true
            IgnoreOneLineBlock = $true
        }

        PSPlaceCloseBrace = @{
            Enable = $true
            NewLineAfter = $false
            IgnoreOneLineBlock = $true
            NoEmptyLineBefore = $true
        }

        PSAvoidLongLines = @{
            Enable = $true
            MaximumLineLength = 170
        }

        PSAvoidUsingDoubleQuotesForConstantString = @{
            Enable = $false
        }

        PSAlignAssignmentStatement = @{
            Enable = $false
            CheckHashtable = $true
        }

        PSAvoidOverwritingBuiltInCmdlets = @{
            Enable = $true
            PowerShellVersion = ""
        }

        PSUseCorrectCasing = @{
            Enable = $true
        }

        PSProvideCommentHelp = @{
            Enable = $false
            ExportedOnly = $true
            BlockComment = $true
            VSCodeSnippetCorrection = $false
            Placement = "before"
        }

        PSUseCompatibleSyntax = @{
            Enable = $true
            TargetVersions = @(
                '5.1'
                '7.0'
            )
        }

        PSUseCompatibleCommands = @{
            Enable = $true
            TargetProfiles = @(
                "win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework"
                "win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core.json"
                "ubuntu_x64_18.04_7.0.0_x64_3.1.2_core.json"
            )
            ProfileDirPath = ""
            IgnoreCommands = @(
                "Invoke-Pester"
                "BeforeDiscovery"
                "BeforeAll"
                "BeforeEach"
                "Describe"
                "Context"
                "It"
                "Mock"
                "Should"
            )
        }

        PSUseCompatibleTypes = @{
            Enable = $true
            TargetProfiles = @(
                "win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework"
                "win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core.json"
                "ubuntu_x64_18.04_7.0.0_x64_3.1.2_core.json"
            )
            ProfileDirPath = ""
            IgnoreTypes = @()
        }
    }
}