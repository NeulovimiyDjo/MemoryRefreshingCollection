using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplyInstallService.Logic
{
    public class PsExecutor
    {
        private Process _process;

        public event NewOutputReceivedHandler OnNewOutputReceived;
        public string WorkingDir { get; set; } = null;

        public Task<ProcessResult> RunPs1File(string filePath, Dictionary<string, string> arguments)
        {
            ProcessStartInfo startInfo = new();
            startInfo.FileName = @"powershell.exe";
            startInfo.WorkingDirectory = WorkingDir ?? Path.GetDirectoryName(filePath);
            startInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{Path.GetFullPath(filePath)}\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            if (arguments.Count > 0)
            {
                string argumentsStr = string.Join(" ", arguments.Select(x => $"-{x.Key} \"{x.Value}\""));
                startInfo.Arguments += $" {argumentsStr}";
            }

            _process = new();
            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;

            StringBuilder output = new();
            _process.OutputDataReceived += (sender, eventArgs) =>
            {
                output.AppendLine(eventArgs.Data);
                OnNewOutputReceived?.Invoke(eventArgs.Data);
            };
            _process.ErrorDataReceived += (sender, eventArgs) =>
            {
                output.AppendLine(eventArgs.Data);
                OnNewOutputReceived?.Invoke(eventArgs.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            var tcs = new TaskCompletionSource<ProcessResult>();
            _process.Exited += (sender, eventArgs) =>
            {
                ProcessResult res = new()
                {
                    ExitCode = _process.ExitCode,
                    Output = output.ToString(),
                };
                _process.Dispose();
                tcs.SetResult(res);
            };

            return tcs.Task;
        }
    }
}