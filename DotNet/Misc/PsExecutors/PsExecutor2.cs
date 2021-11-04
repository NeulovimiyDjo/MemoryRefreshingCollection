using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PsExecutors
{
    public class PsExecutor2
    {
        private object _locker = new();
        public string RunPs1File(string filePath, Dictionary<string, object> arguments)
        {
            ProcessStartInfo startInfo = new();
            startInfo.FileName = @"powershell.exe";
            startInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
            startInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"./{Path.GetFileName(filePath)}\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            using Process process = new();
            process.StartInfo = startInfo;

            if (arguments.Count > 0)
            {
                string argumentsStr = string.Join(" ", arguments.Select(x => $"-{x.Key} \"{x.Value}\""));
                process.StartInfo.Arguments += $" {argumentsStr}";
            }

            StringBuilder output = new();
            process.OutputDataReceived += (sender, eventArgs) => { lock (_locker) { output.AppendLine(eventArgs.Data); }; OutputWriter.WriteToLog(eventArgs.Data); };
            process.ErrorDataReceived += (sender, eventArgs) => { lock (_locker) { output.AppendLine(eventArgs.Data); }; OutputWriter.WriteToLog(eventArgs.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return output.ToString();
        }
    }
}
