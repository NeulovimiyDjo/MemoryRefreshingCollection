function GenerateKeytabFile() {
    $keytabFileUserId = "user"
    $keytabFileUserPass = "pass"

    "#!/bin/bash
    set -e
    cd `"`$(dirname `"`$(readlink -f `"`${BASH_SOURCE[0]}`")`")`"
    user_id=$keytabFileUserId
    read user_pass
    echo -e `"add_entry -password -p `$user_id -k 1 -e arcfour-hmac\n`$user_pass\nadd_entry -password -p `$user_id -k 1 -e aes128-cts\n`$user_pass\nadd_entry -password -p `$user_id -k 1 -e aes256-cts\n`$user_pass\nwkt ./service.keytab`" | ktutil
    ".Replace("`r`n", "`n") | Out-File -FilePath "$pwd/tmp/gen_keytab.sh"

    $keytabFileUserPass | bash "$pwd/tmp/gen_keytab.sh"
    if ($LASTEXITCODE -ne 0) { throw "Keytab generation command failed" }

    $res = "set -e; echo -e `"rkt ./tmp/service.keytab\nlist`" | ktutil" | bash
    if ($LASTEXITCODE -ne 0) { throw "Keytab generation check command failed" }
    Write-Host $res
    if (-not $res -Match $keytabFileUserId) { throw "Keytab generation check content failed" }
}
