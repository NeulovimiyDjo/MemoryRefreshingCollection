using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace WSDLToolGenerators
{
    [Generator]
    public class MyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!System.Diagnostics.Debugger.IsAttached)
//            {
//                System.Diagnostics.Debugger.Launch();
//            }
//#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                "build_property.MSBuildProjectDirectory", out string projectDirectory) == false)
            {
                throw new ArgumentException("MSBuildProjectDirectory");
            }

            string configText = context.AdditionalFiles
                .First(e => e.Path.EndsWith("generatorsettings.json"))
                .GetText(context.CancellationToken)
                .ToString();
            JObject config = JObject.Parse(configText);

            string workDir = GetDirPath(projectDirectory, config, "WorkDir");
            CSCreator.ExtractWSDL("DoSomethingService.wsdl", workDir);
            string generatedServiceInterfaceContent = CSCreator.CreateServiceInterfaceClassFromWSDL("DoSomethingService", workDir);
            string generatedServiceDummyImplementationContent = CSCreator.CreateServiceDummyImplementationClass(generatedServiceInterfaceContent);
            string generatedServiceProxyContent = CSCreator.CreateServiceProxyClassFromWSDL("DoSomethingService", workDir);

            string outDir = GetDirPath(projectDirectory, config, "OutDir");
            File.Copy(@$"{workDir}\DoSomethingService.wsdl", @$"{outDir}\DoSomethingService.wsdl", true);
            File.WriteAllText(@$"{outDir}\IDoSomethingService.cs", $"{generatedServiceInterfaceContent}");
            File.WriteAllText(@$"{outDir}\DoSomethingService.cs", $"{generatedServiceDummyImplementationContent}");
            File.WriteAllText(@$"{outDir}\DoSomethingServiceProxy.cs", $"{generatedServiceProxyContent}");
            context.AddSource("IDoSomethingService", $"{generatedServiceInterfaceContent}");
            context.AddSource("DoSomethingService", $"{generatedServiceDummyImplementationContent}");
        }

        private static string GetDirPath(string projectDirectory, JObject config, string dirConfigKey)
        {
            string dir = config[dirConfigKey].ToString().Replace("/", "\\");
            string fullDir = Path.Combine(projectDirectory, dir);
            Directory.CreateDirectory(fullDir);
            return fullDir;
        }
    }
}