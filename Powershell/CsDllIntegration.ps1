
Add-Type -AssemblyName System #.NET assembly
Add-Type -AssemblyName System.Core
Add-Type -AssemblyName System
Add-Type -Path "$PSScriptRoot\ICSharpCode.AvalonEdit.dll"

$res = [System.BitConverter]::ToString([ICSharpCode.AvalonEditSomeClass]::SomeFunction("123param"));
return $res