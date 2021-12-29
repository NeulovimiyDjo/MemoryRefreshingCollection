
function Warn([string[]]$message) {
	foreach ($line in $message) {
		Write-Host -ForegroundColor yellow "$(Get-Date) WARN $line"
	}
}

function CheckIsInteractive {
    if (![Environment]::UserInteractive) {
        return $false
    }
    foreach ($arg in [Environment]::GetCommandLineArgs()) {
        if ($arg -like '-NonI*') {
            return $false
        }
    }
    return $true
}

function CreateCredential([string]$userName, [string]$userPassword) {
	return New-Object -typename System.Management.Automation.PSCredential -argumentlist $userName, (ConvertTo-SecureString -String $userPassword -AsPlainText -Force)
}


function DisableCertCheck {
Add-Type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    Write-Host "Certificate check disabled"
}

function DisableCertCheck2 {
    $Provider = New-Object Microsoft.CSharp.CSharpCodeProvider
    $Compiler = $Provider.CreateCompiler()
    $Params = New-Object System.CodeDom.Compiler.CompilerParameters
    $Params.GenerateExecutable = $false
    $Params.GenerateInMemory = $true
    $Params.IncludeDebugInformation = $false
    $Params.ReferencedAssemblies.Add("System.DLL") > $null
    $TASource=@'
namespace Local.ToolkitExtensions.Net.CertificatePolicy
{
    public class TrustAll : System.Net.ICertificatePolicy
    {
        public bool CheckValidationResult(System.Net.ServicePoint sp,System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Net.WebRequest req, int problem)
        {
            return true;
        }
    }
}
'@ 
    $TAResults=$Provider.CompileAssemblyFromSource($Params,$TASource)
    $TAAssembly=$TAResults.CompiledAssembly
    ## We create an instance of TrustAll and attach it to the ServicePointManager
    $TrustAll = $TAAssembly.CreateInstance("Local.ToolkitExtensions.Net.CertificatePolicy.TrustAll")
    [System.Net.ServicePointManager]::CertificatePolicy = $TrustAll
}

function DisableCertCheck3 {
    if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
        $certCallback=@"
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
public class ServerCertificateValidationCallback
{
    public static void Ignore()
    {
        if(ServicePointManager.ServerCertificateValidationCallback ==null)
        {
            ServicePointManager.ServerCertificateValidationCallback += 
                delegate
                (
                    Object obj, 
                    X509Certificate certificate, 
                    X509Chain chain, 
                    SslPolicyErrors errors
                )
                {
                    return true;
                };
        }
    }
}
"@
        Add-Type $certCallback
    }
    [ServerCertificateValidationCallback]::Ignore();
}

function DisableCertCheck4 {
    class TrustAllCertsPolicy : System.Net.ICertificatePolicy {
        [bool] CheckValidationResult([System.Net.ServicePoint] $a,
                                    [System.Security.Cryptography.X509Certificates.X509Certificate] $b,
                                    [System.Net.WebRequest] $c,
                                    [int] $d) {
            return $true
        }
    }
    [System.Net.ServicePointManager]::CertificatePolicy = [TrustAllCertsPolicy]::new()
}

function DisableCertCheck5 {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
}

function DisableCertCheck6 {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {
        param(
            [object]$sender,
            [Security.Cryptography.X509Certificates.X509Certificate] $certificate, 
            [Security.Cryptography.X509Certificates.X509Chain] $chain, 
            [Net.Security.SslPolicyErrors] $sslPolicyErrors
        )
        # Implement your custom logic here
        $true
    }
}

function AssemblyLoad1 {
    $source = @"
using Ionic.Zip;
public static class Helper
{
        public static ZipFile GetNewFile(string fileName)
        {       
            return new ZipFile(fileName);
        }
}
"@

    Add-Type -Path "2.cs" -ReferencedAssemblies "Ionic.Zip.dll" -OutputAssembly "T.dll"
    Add-Type -Path "T.dll"

    $var = [Helper]::GetNewFile("aaa")
    $var.AlternateEncoding
}

function AssemblyLoad2 {
    $candidateAssembly =  "C:\My2ndProject\bin\Debug\My1stProject.exe"

    # Load your target version of the assembly (these were from the NuGet package, and 
    # have a version incompatible with what My2ndProject.exe expects)
    [System.Reflection.Assembly]::LoadFrom("C:\My2ndProject\bin\Debug\Spring.Aop.dll")
    [System.Reflection.Assembly]::LoadFrom("C:\My2ndProject\bin\Debug\Spring.Core.dll")
    [System.Reflection.Assembly]::LoadFrom("C:\My2ndProject\bin\Debug\Spring.Data.dll")

    # Method to intercept resolution of binaries
    $onAssemblyResolveEventHandler = [System.ResolveEventHandler] {
        param($sender, $e)

        Write-Host "ResolveEventHandler: Attempting FullName resolution of $($e.Name)" 
        foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
            if ($assembly.FullName -eq $e.Name) {
                Write-Host "Successful FullName resolution of $($e.Name)" 
                return $assembly
            }
        }

        Write-Host "ResolveEventHandler: Attempting name-only resolution of $($e.Name)" 
        foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
            # Get just the name from the FullName (no version)
            $assemblyName = $assembly.FullName.Substring(0, $assembly.FullName.IndexOf(", "))

            if ($e.Name.StartsWith($($assemblyName + ","))) {

                Write-Host "Successful name-only (no version) resolution of $assemblyName" 
                return $assembly
            }
        }

        Write-Host "Unable to resolve $($e.Name)" 
        return $null
    }

    # Wire-up event handler
    [System.AppDomain]::CurrentDomain.add_AssemblyResolve($onAssemblyResolveEventHandler)

    # Load into app domain
    $assembly = [System.Reflection.Assembly]::LoadFrom($candidateAssembly) 

    try
    {
        # this ensures that all dependencies were loaded correctly
        $assembly.GetTypes() 
    } 
    catch [System.Reflection.ReflectionTypeLoadException] 
    { 
        Write-Host "Message: $($_.Exception.Message)" 
        Write-Host "StackTrace: $($_.Exception.StackTrace)"
        Write-Host "LoaderExceptions: $($_.Exception.LoaderExceptions)"
    }
}




$ErrorActionPreference = "Stop"
Export-ModuleMember -Function Warn
Export-ModuleMember -Function DisableCertCheck
Export-ModuleMember -Function CheckIsInteractive
Export-ModuleMember -Function CreateCredential



function CreateConfigs([PSCustomObject]$jsonConfigs) {
    foreach ($prop in $jsonConfigs.PSObject.Properties) {
        HandleNode $prop.Value $prop.Name
    }
}
function HandleNode([PSCustomObject]$node, [string]$path) {
    if ($node.PSObject.Properties.Name -contains "ItemName") {
        [PSCustomObject]@{
            Path = $path
            Config = $node
        }
    } else {
        foreach ($prop in $node.PSObject.Properties) {
            HandleNode $prop.Value "$path\$($prop.Name)"
        }
    }
}



Get-Module | Remove-Module
$Modules = "..\ModulesFolder\*.psm1"
Resolve-Path -Path $Modules | ForEach-Object { Import-Module $_}

$CheckUrl = "$($TestUrl)/checkpage"
try
{
	$res = (Invoke-WebRequest $CheckUrl)
	Write-Host "Response status code: $($res.StatusCode)"
}
catch
{
	Write-Host "Connection failed"
	throw
}

$machineName = $Env:SOME_MACHINE_NAME_OR_IP
$Credential = CreateCredential "user" "password"
$Session = New-PSSession -ComputerName "$machineName" -Credential $Credential
$Session | Remove-PSSession


function CheckPSSessionAccess([string]$computerName) {
	Write-Host "Checking access to $computerName"
    $session = New-PSSession -Credential $credential -ComputerName $computerName -UseSSL -SessionOption $sessionOption -ErrorAction SilentlyContinue
    if ($null -eq $session) {
        Write-Host "$computerName inaccessible"
    } else {
        Invoke-Command -Session $session -ScriptBlock { $hostName = (hostname); Write-Host "hello from $hostName" }
        $session | Remove-PSSession
    }
    Write-Host ""
}
 
function CheckTCPPort([string]$computerName, [int]$port) {
	Write-Host "Checking tcp port $computerName : $port"
    $client = New-Object Net.Sockets.TcpClient
    $client.Connect($computerName, $port)
    if ($client.Connected) {
        Write-Host "Successfully tcp-connected to ${computerName}:${port}"
    } else {
        Write-Host "Failed to tcp-connect to ${computerName}:${port}"
    }
    $client.Dispose()
    Write-Host ""
}

#Check encoding
$sr = [System.IO.StreamReader]::new(".\file.xd", [System.Text.Encoding]::ASCII, $true); $null = $sr.Peek(); $enc = $sr.CurrentEncoding.BodyName; $sr.Dispose(); $enc -eq "utf-8"
[System.Text.Encoding]::Default.Codepage = Out-File -encoding default
[System.Text.Encoding]::GetEncoding("windows-1251")

#SetEnvVar
[Environment]::SetEnvironmentVariable("Path", $env:Path + "c:\some\path", [System.EnvironmentVariableTarget]::Machine)