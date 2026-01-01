using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProcessArgsParserTest
{
    internal static class ProcessArgsParser
    {
        public static void GetProcesses()
        {
            var processes = Process.GetProcesses().Where(p => p.ProcessName == "XXX" && !p.HasExited);
            foreach (Process process in processes)
            {
                string[] args = ProcessHelper.GetArgs(process.Id);
                var seenMemoryUsed = process.WorkingSet64;
                var seenProcessorTimeMs = (long)process.TotalProcessorTime.TotalMilliseconds;
            }
        }

        public static string[] GetArgs(int pid)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetArgsLinux(pid);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetArgsWindows(pid);
            else
                throw new Exception($"Invalid OS while getting args for process pid={pid}");
        }

        private static string[] GetArgsLinux(int pid)
        {
            string cmdLine = File.ReadAllText($"/proc/{pid}/cmdline");
            return cmdLine.Trim('\0').Split('\0');
        }

        private static string[] GetArgsWindows(int pid)
        {
            string cmd = @$"(Get-WmiObject Win32_Process -Filter 'ProcessID = {pid}').CommandLine";
            string cmdLine = ExecuteInPowershell(cmd);
            return SplitQuotedBySpace(cmdLine.Trim());
        }

        private static string ExecuteInPowershell(string cmd)
        {
            Process p = new();
            p.StartInfo.FileName = "powershell.exe";
            p.StartInfo.ArgumentList.Add(cmd);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
            string res = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return res.Trim();
        }

        private static string[] SplitQuotedBySpace(string cmdLine)
        {
            return cmdLine.Split('"')
                .Select((element, index) =>
                    index % 2 == 0 // If even index
                        ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) // Split the item
                        : new string[] { element }) // Keep the entire item
                .SelectMany(element => element)
                .ToArray();
        }
    }
}
