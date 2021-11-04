using module "..\Classes\Logger.psm1"

Describe "Logger" {
    It "Writes all levels when verbosity is Trace" {
        $logger = [Logger]::new("Trace")
        ($logger.Error("some error msg") *>&1) | Should -BeLikeExactly "* ERROR some error msg"
        ($logger.Warn("some warn msg") *>&1) | Should -BeLikeExactly "* WARN some warn msg"
        ($logger.Info("some info msg") *>&1) | Should -BeLikeExactly "* INFO some info msg"
        ($logger.Debug("some debug msg") *>&1) | Should -BeLikeExactly "* DEBUG some debug msg"
        ($logger.Trace("some trace msg") *>&1) | Should -BeLikeExactly "* TRACE some trace msg"
    }

    It "Writes Error, Warn and Info when verbosity is Info" {
        $logger = [Logger]::new("Info")
        ($logger.Error("some error msg") *>&1) | Should -BeLikeExactly "* ERROR some error msg"
        ($logger.Warn("some warn msg") *>&1) | Should -BeLikeExactly "* WARN some warn msg"
        ($logger.Info("some info msg") *>&1) | Should -BeLikeExactly "* INFO some info msg"
        ($logger.Debug("some debug msg") *>&1) | Should -Be $null
        ($logger.Trace("some trace msg") *>&1) | Should -Be $null
    }

    It "Does not write anything when verbosity is None" {
        $logger = [Logger]::new("None")
        ($logger.Error("some error msg") *>&1) | Should -Be $null
        ($logger.Warn("some warn msg") *>&1) | Should -Be $null
        ($logger.Info("some info msg") *>&1) | Should -Be $null
        ($logger.Debug("some debug msg") *>&1) | Should -Be $null
        ($logger.Trace("some trace msg") *>&1) | Should -Be $null
    }

    It "Correctly adds strings to hide and sanitizes output" {
        $logger = [Logger]::new("Trace")
        $logger.AddStringsToHideInLogs(@("Secret", "secretval"))
        $logger.AddStringsToHideInLogs("Secret")
        $logger.AddStringsToHideInLogs("SecretVal2")
        $logger.StringsToHideInLogs | Should -BeExactly @("SecretVal2", "secretval", "Secret")
        $escapedExpectedLogPart = [Regex]::Escape(" TRACE Some trace containing '********' and [********] and********.")
        ($logger.Trace("Some trace containing 'Secret' and [secretval] andSecretVal2.") *>&1) |
            Should -MatchExactly ".*$escapedExpectedLogPart"
    }

    It "Displays array correctly" {
        [Logger]::DisplayArray(@()) | Should -BeExactly "[]"
        [Logger]::DisplayArray("single-val") | Should -BeExactly "['single-val']"
        [Logger]::DisplayArray(@("val1", "val2", "val3 val4")) | Should -BeExactly "['val1', 'val2', 'val3 val4']"
        [Logger]::DisplayArray(@(11, 22)) | Should -BeExactly "['11', '22']"
    }
}