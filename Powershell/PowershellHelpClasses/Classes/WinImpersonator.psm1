using module "..\Classes\Logger.psm1"
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingUsernameAndPasswordParams", "")]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "")]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "")]
param()
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$logonUserSignature =
@'
[DllImport("advapi32.dll")]
public static extern bool LogonUser(
    String lpszUserName,
    String lpszDomain,
    String lpszPassword,
    int dwLogonType,
    int dwLogonProvider,
    ref IntPtr phToken);
'@
$AdvApi32 = Add-Type -MemberDefinition $logonUserSignature -Name "AdvApi32" -Namespace "PsInvoke.NativeMethods" -PassThru

$closeHandleSignature =
@'
[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
public static extern bool CloseHandle(IntPtr handle);
'@
$Kernel32 = Add-Type -MemberDefinition $closeHandleSignature -Name "Kernel32" -Namespace "PsInvoke.NativeMethods" -PassThru

function Impersonate {
    param(
        [string]$UserName,
        [string]$Domain,
        [string]$Password,
        [ScriptBlock]$ScriptBlock,
        [Logger]$Logger
    )

    try {
        $context = $null
        $identityName = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
        $Logger.Debug("Current Identity: $identityName")

        $Logon32ProviderDefault = 0
        $Logon32LogonInteractive = 2
        $tokenHandle = [IntPtr]::Zero
        $unmanagedString = [IntPtr]::Zero;
        $success = $false

        try {
            $securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
            $unmanagedString = [System.Runtime.InteropServices.Marshal]::SecureStringToGlobalAllocUnicode($securePassword);
            $success = $AdvApi32::LogonUser(
                $UserName,
                $Domain,
                [System.Runtime.InteropServices.Marshal]::PtrToStringUni($unmanagedString),
                $Logon32LogonInteractive,
                $Logon32ProviderDefault,
                [ref]$tokenHandle)
        } finally {
            [System.Runtime.InteropServices.Marshal]::ZeroFreeGlobalAllocUnicode($unmanagedString);
        }

        if (!$success) {
            $retVal = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
            throw "Impersonate:LogonUser was unsuccessful. Error code: $retVal"
        }

        $Logger.Debug("Impersonate:LogonUser was successful.")

        $newIdentity = New-Object System.Security.Principal.WindowsIdentity($tokenHandle)
        $context = $newIdentity.Impersonate()

        $identityName = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
        $Logger.Debug("Impersonated Identity: $identityName")

        $Logger.Debug("Executing script block in impersonation mode")
        return & $ScriptBlock
    } catch [System.Exception] {
        throw "$($_.Exception.ToString())"
    } finally {
        if ($null -ne $context) {
            $context.Undo()
        }

        if ($tokenHandle -ne [System.IntPtr]::Zero) {
            $Kernel32::CloseHandle($tokenHandle) | Out-Null
        }
    }
}

$createProcessCode = @"
using System;
using System.Text;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;

public class CreateProcessWithLogon
    {
        private const UInt32 PipeBufferSize = 4096;
        private const UInt32 Infinite = 0xffffffff;

        public static class HandleInformationMask
        {
            public const UInt32 HANDLE_FLAG_INHERIT = 0x00000001;
            public const UInt32 HANDLE_FLAG_PROTECT_FROM_CLOSE = 0x00000002;
        }

        public static class WaitResults
        {
            public const UInt32 WAIT_ABANDONED = 0x00000080;
            public const UInt32 WAIT_OBJECT_0 = 0x00000000;
            public const UInt32 WAIT_TIMEOUT = 0x00000102;
            public const UInt32 WAIT_FAILED = 0xFFFFFFFF;
        }

        public static class LogonFlags
        {
            public const UInt32 LOGON_WITH_PROFILE = 1;
            public const UInt32 LOGON_NETCREDENTIALS_ONLY = 2;
        }

        public static class CreationFlags
        {
            public const UInt32 CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
            public const UInt32 CREATE_DEFAULT_ERROR_MODE = 0x04000000;
            public const UInt32 CREATE_NEW_CONSOLE = 0x00000010;
            public const UInt32 CREATE_NEW_PROCESS_GROUP = 0x00000200;
            public const UInt32 CREATE_NO_WINDOW = 0x08000000;
            public const UInt32 CREATE_PROTECTED_PROCESS = 0x00040000;
            public const UInt32 CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000;
            public const UInt32 CREATE_SECURE_PROCESS = 0x00400000;
            public const UInt32 CREATE_SEPARATE_WOW_VDM = 0x00000800;
            public const UInt32 CREATE_SHARED_WOW_VDM = 0x00001000;
            public const UInt32 CREATE_SUSPENDED = 0x00000004;
            public const UInt32 CREATE_UNICODE_ENVIRONMENT = 0x00000400;
            public const UInt32 DEBUG_ONLY_THIS_PROCESS = 0x00000002;
            public const UInt32 DEBUG_PROCESS = 0x00000001;
            public const UInt32 DETACHED_PROCESS = 0x00000008;
            public const UInt32 EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
            public const UInt32 INHERIT_PARENT_AFFINITY = 0x00010000;
        }

        public static class StartupFlags
        {
            public const int STARTF_USESTDHANDLES = 0x00000100;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct ProcessInformation
        {
            public IntPtr process;
            public IntPtr thread;
            public int processId;
            public int threadId;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
            string userName,
            string domain,
            string password,
            UInt32 logonFlags,
            string applicationName,
            string commandLine,
            UInt32 creationFlags,
            UInt32 environment,
            string currentDirectory,
            ref StartupInfo startupInfo,
            out ProcessInformation processInfo);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, UInt32 nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetHandleInformation(IntPtr hObject, UInt32 dwMask, UInt32 dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, UInt32 nNumberOfBytesToRead, out UInt32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr process, out UInt32 exitCode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentThreadId();

        public class Result
        {
            public int ExitCode;
            public string Output;
        }

        public Result CreateProcess(
            string userName,
            string domain,
            string password,
            UInt32 logonFlags,
            string binary,
            string arguments,
            string currentDirectory,
            UInt32 timeoutMilliseconds = Infinite)
        {
            if (timeoutMilliseconds <= 0)
                timeoutMilliseconds = Infinite;

            GrantAccessToWindowStationAndDesktop(userName);

            IntPtr hChildStd_OUT_Rd;
            IntPtr hChildStd_OUT_Wr;
            if (!CreateAnonymousPipe(out hChildStd_OUT_Rd, out hChildStd_OUT_Wr))
                throw new Exception("CreatePipe failed with error code: " + Marshal.GetLastWin32Error());

            if (!SetHandleInformation(hChildStd_OUT_Rd, HandleInformationMask.HANDLE_FLAG_INHERIT, 0u))
                throw new Exception("SetHandleInformation failed with error code: " + Marshal.GetLastWin32Error());

            StartupInfo startupInfo = new StartupInfo();
            startupInfo.cb = Marshal.SizeOf(startupInfo);
            startupInfo.lpReserved = null;
            startupInfo.dwFlags = StartupFlags.STARTF_USESTDHANDLES;
            startupInfo.hStdOutput = hChildStd_OUT_Wr;
            startupInfo.hStdError = hChildStd_OUT_Wr;

            ProcessInformation processInfo;
            bool isStarted = CreateProcessWithLogonW(
                userName,
                domain,
                password,
                logonFlags,
                null,
                binary + " " + arguments,
                CreationFlags.CREATE_UNICODE_ENVIRONMENT | CreationFlags.CREATE_NO_WINDOW,
                (UInt32)0,
                currentDirectory,
                ref startupInfo,
                out processInfo);

            CloseHandle(hChildStd_OUT_Wr);
            if (!isStarted)
            {
                var errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 1326)
                    throw new Exception("Error code: 1326. The user name or password is incorrect. ERROR_ACCOUNT_RESTRICTION.");
                else
                    throw new Exception("Couldn't start process. Exited with code: " + errorCode);
            }

            try
            {
                object sbSyncLock = new object();
                StringBuilder outputSb = new StringBuilder();
                var thread = new System.Threading.Thread(() =>
                {
                    ReadOutputFromPipeToOutputSb(hChildStd_OUT_Rd, Console.OutputEncoding, outputSb, sbSyncLock);
                });
                thread.IsBackground = true;
                thread.Start();

                var waitResult = WaitForSingleObject(processInfo.process, timeoutMilliseconds);
                if (waitResult != WaitResults.WAIT_OBJECT_0)
                {
                    lock(sbSyncLock)
                    {
                        Console.WriteLine("Process failed, partial output before failure:");
                        Console.WriteLine(outputSb.ToString());
                    }
                    if (waitResult == WaitResults.WAIT_TIMEOUT)
                        throw new Exception("Process timed out");
                    else
                        throw new Exception("Process execution failed with code: " + waitResult);
                }

                UInt32 exitCode;
                GetExitCodeProcess(processInfo.process, out exitCode);
                return new Result() { ExitCode = (int)exitCode, Output = outputSb.ToString() };
            }
            finally
            {
                CloseHandle(processInfo.process);
                CloseHandle(processInfo.thread);
                CloseHandle(hChildStd_OUT_Rd);
            }
        }

        public static void GrantAccessToWindowStationAndDesktop(string username)
        {
            IntPtr handle;
            const int WindowStationAllAccess = 0x000f037f;
            handle = GetProcessWindowStation();
            GrantAccess(username, handle, WindowStationAllAccess);
            const int DesktopRightsAllAccess = 0x000f01ff;
            handle = GetThreadDesktop(GetCurrentThreadId());
            GrantAccess(username, handle, DesktopRightsAllAccess);
        }

        private static void GrantAccess(string username, IntPtr handle, int accessMask)
        {
            SafeHandle safeHandle = new NoopSafeHandle(handle);
            GenericSecurity security =
                new GenericSecurity(
                    false, ResourceType.WindowObject, safeHandle, AccessControlSections.Access);

            security.AddAccessRule(
                new GenericAccessRule(
                    new NTAccount(username), accessMask, AccessControlType.Allow));
            security.Persist(safeHandle, AccessControlSections.Access);
        }

        private class GenericAccessRule : AccessRule
        {
            public GenericAccessRule(
                IdentityReference identity, int accessMask, AccessControlType type) :
                base(identity, accessMask, false, InheritanceFlags.None,
                    PropagationFlags.None, type)
            {
            }
        }

        private class GenericSecurity : NativeObjectSecurity
        {
            public GenericSecurity(
                bool isContainer, ResourceType resType, SafeHandle objectHandle,
                AccessControlSections sectionsRequested)
                : base(isContainer, resType, objectHandle, sectionsRequested)
            {
            }

            new public void Persist(SafeHandle handle, AccessControlSections includeSections)
            {
                base.Persist(handle, includeSections);
            }

            new public void AddAccessRule(AccessRule rule)
            {
                base.AddAccessRule(rule);
            }

            #region NativeObjectSecurity Abstract Method Overrides

            public override Type AccessRightType
            {
                get { throw new NotImplementedException(); }
            }

            public override AccessRule AccessRuleFactory(
                System.Security.Principal.IdentityReference identityReference,
                int accessMask, bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AccessControlType type)
            {
                throw new NotImplementedException();
            }

            public override Type AccessRuleType
            {
                get { return typeof(AccessRule); }
            }

            public override AuditRule AuditRuleFactory(
                System.Security.Principal.IdentityReference identityReference, int accessMask,
                bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AuditFlags flags)
            {
                throw new NotImplementedException();
            }

            public override Type AuditRuleType
            {
                get { return typeof(AuditRule); }
            }

            #endregion
        }

        // Handles returned by GetProcessWindowStation and GetThreadDesktop should not be closed
        private class NoopSafeHandle : SafeHandle
        {
            public NoopSafeHandle(IntPtr handle) :
                base(handle, false)
            {
            }

            public override bool IsInvalid
            {
                get { return false; }
            }

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }

        private static bool CreateAnonymousPipe(out IntPtr hReadPipe, out IntPtr hWritePipe)
        {
            SECURITY_ATTRIBUTES lpPipeAttributes = default(SECURITY_ATTRIBUTES);
            lpPipeAttributes.Length = Marshal.SizeOf(lpPipeAttributes);
            lpPipeAttributes.lpSecurityDescriptor = IntPtr.Zero;
            lpPipeAttributes.bInheritHandle = true;
            if (CreatePipe(out hReadPipe, out hWritePipe, lpPipeAttributes, PipeBufferSize))
                return true;
            return false;
        }

        private static void ReadOutputFromPipeToOutputSb(IntPtr hReadPipe, Encoding enc, StringBuilder outputSb, object sbSyncLock)
        {
            byte[] array = new byte[PipeBufferSize];
            UInt32 lpNumberOfBytesRead;
            while (true)
            {
                bool success = ReadFile(hReadPipe, array, PipeBufferSize, out lpNumberOfBytesRead, IntPtr.Zero);
                if (!success || lpNumberOfBytesRead == 0)
                    break;
                string newOutputChunk = enc.GetString(array, 0, (int)lpNumberOfBytesRead);
                lock(sbSyncLock)
                {
                    outputSb.Append(newOutputChunk);
                }
            }
        }
    }
"@
Add-Type -TypeDefinition $createProcessCode -Language CSharp

function StartAndWaitProcessWithLogon {
    param(
        [string]$UserName,
        [string]$Domain,
        [string]$Password,
        [string]$Binary,
        [string]$Arguments,
        [int]$TimeoutSeconds
    )

    $executor = New-Object CreateProcessWithLogon
    return $executor.CreateProcess(
        $UserName,
        $Domain,
        $Password,
        [CreateProcessWithLogon+LogonFlags]::LOGON_NETCREDENTIALS_ONLY,
        $Binary,
        $Arguments,
        [System.IO.Path]::GetDirectoryName($Binary),
        $TimeoutSeconds * 1000);
}
Export-ModuleMember #none

class WinImpersonator {
    [ValidateNotNullOrEmpty()][string]$UserName
    [ValidateNotNullOrEmpty()][string]$Password

    [ValidateNotNull()][Logger]$Logger
    [ValidateNotNullOrEmpty()][string]$Domain
    [ValidateNotNullOrEmpty()][string]$UserNameOnly

    WinImpersonator([PSCustomObject]$config) {
        $this.Init($config, (New-Object Logger([Verbosity]::Trace)))
    }
    WinImpersonator([PSCustomObject]$config, [Logger]$logger) {
        $this.Init($config, $logger)
    }
    hidden [void]Init([PSCustomObject]$config, [Logger]$logger) {
        $this.UserName = $config.UserName
        $this.Password = $config.Password

        $this.Logger = $logger
        $userNameParts = $this.UserName.Split('\')
        if ($userNameParts.Count -ne 2) {
            throw "UserName $($this.UserName) doesn't contain domain or invalid in some other way"
        }
        $this.Domain = $userNameParts[0]
        $this.UserNameOnly = $userNameParts[1]
    }

    [object]ExecuteScriptBlockAsUser([ScriptBlock]$scriptBlock) {
        $this.Logger.Debug("Executing script block as user '$($this.UserName)'")
        $params = @{
            UserName = $this.UserNameOnly
            Domain = $this.Domain
            Password = $this.Password
            ScriptBlock = $scriptBlock
            Logger = $this.Logger
        }
        return Impersonate @params
    }

    [string]ExecuteProcessAsUser([string]$processPath, [string[]]$arguments, [int]$timeout) {
        $this.Logger.Debug("Executing as user='$($this.UserName)', process='$processPath', arguments=$([Logger]::DisplayArray($arguments)), timeout='$timeout'")

        $params = @{
            UserName = $this.UserNameOnly
            Domain = $this.Domain
            Password = $this.Password
            Binary = $processPath
            Arguments = $arguments
            TimeoutSeconds = $timeout
        }
        $result = StartAndWaitProcessWithLogon @params

        $this.Logger.Debug("ExitCode='$($result.ExitCode)'")
        if ($result.ExitCode -ne 0) {
            $this.Logger.Error("Total ProcessRunner output:`n$($result.Output)")
            throw "ExecuteProcessAsUser failed: ExitCode='$($result.ExitCode)'"
        }

        return $result.Output.Trim()
    }
}
