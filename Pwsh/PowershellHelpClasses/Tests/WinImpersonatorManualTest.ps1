using module "..\Classes\WinImpersonator.psm1"

$config = [PSCustomObject]@{
    UserName = "SomeDomain\SomeUser"
    Password = "SomePassword"
}
$impersonator = [WinImpersonator]::new($config)

$result = $impersonator.ExecuteScriptBlockAsUser({ Write-Host "123"; return "bla bla" })
Write-Host "ExecuteScriptBlockAsUser result='$result'"

$output = $impersonator.ExecuteProcessAsUser("C:\Windows\System32\cmd.exe", @("/c", "echo 456"), 0)
Write-Host "ExecuteProcessAsUser output='$output'"
