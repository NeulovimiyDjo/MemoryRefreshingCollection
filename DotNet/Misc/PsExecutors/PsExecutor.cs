using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PsExecutors
{
    public class PsExecutor
    {
        public string RunPs1File(string filePath, Dictionary<string, object> arguments)
        {
            //InitialSessionState runspaceConfiguration = InitialSessionState.Create();
            //using Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            //runspace.Open();

            //Environment.CurrentDirectory = Path.GetDirectoryName(filePath);
            using PowerShell ps = PowerShell.Create(RunspaceMode.NewRunspace);
            ps.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(filePath));

            ps.AddCommand("Set-ExecutionPolicy")
                .AddParameter("Scope", "Process")
                .AddParameter("ExecutionPolicy", "Bypass")
                .Invoke();


            //var script = "param($param1) $output = 'testing params in C#:' + $param1; $output; $loc = Get-Location; $loc";
            //ps.AddScript(script);
            //ps.AddParameter("param1", "ParamsinC#");


            ps.AddScript(File.ReadAllText(filePath))
                .AddParameters(arguments);

            ps.Streams.Debug.DataAdded += DataAdded;
            ps.Streams.Information.DataAdded += DataAdded;
            ps.Streams.Warning.DataAdded += DataAdded;
            ps.Streams.Error.DataAdded += DataAdded;

            var psOutput = ps.Invoke();

            var errors = ps.Streams.Error;
            if (errors.Count > 0)
            {
                Exception ex = errors[0].Exception;
                ps.Streams.ClearStreams();
                throw ex;
            }

            var result = new StringBuilder();
            foreach (var line in psOutput)
            {
                if (line != null)
                    result.AppendLine(line.ToString());
            }

            return result.ToString();
        }

        private void DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<InformationRecord>;
            var currentStreamRecord = streamObjectsReceived[e.Index];
            OutputWriter.WriteToLog(currentStreamRecord.MessageData.ToString());
        }
    }
}
