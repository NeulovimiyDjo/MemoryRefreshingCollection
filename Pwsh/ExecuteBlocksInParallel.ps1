function ExecuteScriptBlocksInParallel([ScriptBlock[]]$scriptBlocksList) {
    $jobsList = @()
    foreach ($scriptBlock in $scriptBlocksList) {
        $jobsList += Start-Job -ScriptBlock $scriptBlock
    }
    foreach ($job in $jobsList) {
        $output = $job | Receive-Job -Wait -ErrorAction "SilentlyContinue"
        Write-Host "Output: '$output' $($output.Count)"
    }
    $jobsList | Receive-Job
    $jobsList | Remove-Job
}

$sb = {
    "11"
    sleep 3
    "22"
    throw "akkkk"
    "33"
}
ExecuteScriptBlocksInParallel $sb, $sb, $sb