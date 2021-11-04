Invoke-Command -ComputerName "SomeComputerName" -ScriptBlock { Get-Process | Foreach-Object {Write-Host "$($_.Id)   $($_.Path)"} }
systeminfo /s:"SomeComputerName"
tasklist /s:"SomeComputerName"
wmic /node:"SomeComputerName" process where (processid=4567) get parentprocessid
taskkill /s:"SomeComputerName" /PID 123


$data = [System.IO.File]::ReadAllBytes("my_binary_file.dll"); [Convert]::ToBase64String($data)

$b64data = Read-Host
$data = [Convert]::FromBase64String($b64data); [System.IO.File]::WriteAllBytes("my_binary_file.dll", $data)