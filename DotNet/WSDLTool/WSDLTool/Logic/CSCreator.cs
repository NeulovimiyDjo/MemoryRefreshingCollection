using System.IO;
using System.Diagnostics;
using System;

namespace WSDLTool.Logic
{
    public static class CSCreator
    {
        public static string CreateServiceProxyClassFromWSDL(string serviceNamespace, string wsdl)
        {
            string arg = $"/noConfig /namespace:*,{serviceNamespace}";
            return RunSvcUtil(arg, wsdl);
        }

        public static string CreateServiceInterfaceClassFromWSDL(string serviceNamespace, string wsdl)
        {
            string arg = @$"/sc /namespace:*,{serviceNamespace}";
            return RunSvcUtil(arg, wsdl);
        }

        private static string RunSvcUtil(string arg, string wsdl)
        {
            string workDir = Directory.GetCurrentDirectory();
            File.WriteAllText(@$"{workDir}\tmp_generated.wsdl", wsdl);

            ProcessStartInfo startInfo = new();
            startInfo.FileName = Path.Combine(workDir, "SvcUtil.exe");
            startInfo.WorkingDirectory = workDir;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = arg + @$" /out:""{workDir}\tmp_generated.cs""" + @$" ""{workDir}\tmp_generated.wsdl""";
            startInfo.RedirectStandardError = true;
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            string errors = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errors))
                throw new InvalidOperationException(errors);

            string res = File.ReadAllText(@$"{workDir}\tmp_generated.cs");
            File.Delete(@$"{workDir}\tmp_generated.cs");
            File.Delete(@$"{workDir}\tmp_generated.wsdl");
            return res;
        }
    }
}
