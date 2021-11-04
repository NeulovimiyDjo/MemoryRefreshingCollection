$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$SanitizeIgnore = @(
    ""
)
Export-ModuleMember #none

enum Verbosity {
    None = 0
    Error = 1
    Warn = 2
    Info = 3
    Debug = 4
    Trace = 5
}

class Logger {
    [Verbosity]$Verbosity
    [AllowEmptyCollection()][string[]]$StringsToHideInLogs

    Logger([Verbosity]$verbosity) {
        $this.Verbosity = $verbosity
        $this.StringsToHideInLogs = @()
    }

    [void]Error([string[]]$message) {
        if ([int]$this.Verbosity -ge 1) {
            foreach ($line in $message) {
                Write-Host -ForegroundColor Red "$([Logger]::GetDateTime()) ERROR $($this.Sanitize($line))"
            }
        }
    }

    [void]Warn([string[]]$message) {
        if ([int]$this.Verbosity -ge 2) {
            foreach ($line in $message) {
                Write-Host -ForegroundColor Yellow "$([Logger]::GetDateTime()) WARN $($this.Sanitize($line))"
            }
        }
    }

    [void]Info([string[]]$message) {
        if ([int]$this.Verbosity -ge 3) {
            foreach ($line in $message) {
                Write-Host -ForegroundColor Green "$([Logger]::GetDateTime()) INFO $($this.Sanitize($line))"
            }
        }
    }

    [void]Debug([string[]]$message) {
        if ([int]$this.Verbosity -ge 4) {
            foreach ($line in $message) {
                Write-Host -ForegroundColor Cyan "$([Logger]::GetDateTime()) DEBUG $($this.Sanitize($line))"
            }
        }
    }

    [void]Trace([string[]]$message) {
        if ([int]$this.Verbosity -ge 5) {
            foreach ($line in $message) {
                Write-Host -ForegroundColor Gray "$([Logger]::GetDateTime()) TRACE $($this.Sanitize($line))"
            }
        }
    }

    [void]AddStringsToHideInLogs([string[]]$stringsToHide) {
        $newStringsToHideInLogs = $this.StringsToHideInLogs + $stringsToHide
        $this.StringsToHideInLogs = $newStringsToHideInLogs |
            Select-Object -Unique |
            Where-Object { $_ -notin $SanitizeIgnore } |
            Sort-Object -Property Length -Desc
    }

    [string]Sanitize([string]$value) {
        $result = $value
        foreach ($stringToHide in $this.StringsToHideInLogs) {
            $result = $result.Replace($stringToHide, "********")
        }
        return $result
    }

    static [string]DisplayArray([string[]]$array) {
        $commaJoinedQuotedValues = ($array | ForEach-Object { "'$_'" }) -Join ", "
        return "[$commaJoinedQuotedValues]"
    }

    static [string]GetDateTime() {
        return [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss")
    }
}
