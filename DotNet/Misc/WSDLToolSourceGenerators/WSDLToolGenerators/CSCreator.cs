using System.IO;
using System.Diagnostics;
using SharedProjects.SharedHelpers.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace WSDLToolGenerators
{
    public static class CSCreator
    {
        private const string SvcUtil = "SvcUtil.exe";

        public static string CreateServiceProxyClassFromWSDL(string serviceName, string workDir = null)
        {
            string arg = $"/noConfig /namespace:*,{serviceName}Namespace";
            return RunSvcUtil(arg, serviceName, workDir);
        }

        public static string CreateServiceInterfaceClassFromWSDL(string serviceName, string workDir = null)
        {
            string arg = @$"/sc /namespace:*,{serviceName}Namespace";
            return RunSvcUtil(arg, serviceName, workDir);
        }

        public static void ExtractWSDL(string wsdlPath, string workDir)
        {
            workDir = Path.GetFullPath(workDir ?? ".");
            ExctractFileFromResources(workDir, wsdlPath);
        }

        private static string RunSvcUtil(string arg, string serviceName, string workDir)
        {
            workDir = Path.GetFullPath(workDir ?? ".");
            ExctractFileFromResources(workDir, SvcUtil);

            Process p = new();
            p.StartInfo.FileName = Path.Combine(workDir, SvcUtil);
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(workDir);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments = arg + @$" /out:""{workDir}\tmp_{serviceName}.cs""" + @$" ""{workDir}\{serviceName}.wsdl""";
            p.Start();
            p.WaitForExit();

            string res = File.ReadAllText(@$"{workDir}\tmp_{serviceName}.cs");
            File.Delete(@$"{workDir}\{SvcUtil}");
            File.Delete(@$"{workDir}\tmp_{serviceName}.cs");
            return res;
        }

        private static void ExctractFileFromResources(string workDir, string resourceName)
        {
            string filePath = Path.Combine(workDir, resourceName);
            using Stream resourceStream = ResourcesManager.GetStream(resourceName);
            using Stream fileSteram = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            resourceStream.CopyTo(fileSteram);
        }


        public static string CreateServiceDummyImplementationClass(string generatedServiceInterfaceContent)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(generatedServiceInterfaceContent);
            SyntaxNode syntaxRoot = syntaxTree.GetRoot();
            InterfaceDeclarationSyntax interfaceDeclaration = syntaxRoot
                .DescendantNodes().OfType<InterfaceDeclarationSyntax>().First();

            string interfaceName = interfaceDeclaration.Identifier.Text;
            string className = interfaceName.Remove(0, 1);
            string @namespace = $"{className}Namespace";

            //Start
            string res =
@$"namespace {@namespace}
{{
    public class {className}: {interfaceName}
    {{";

            IEnumerable<MethodDeclarationSyntax> methods = interfaceDeclaration.Members
                .Where(x => x.Kind() == SyntaxKind.MethodDeclaration)
                .Select(x => (MethodDeclarationSyntax)x);
            foreach (MethodDeclarationSyntax method in methods)
            {
                string methodName = method.Identifier.Text;
                string returnType = method.ReturnType.ToString();

                ParameterSyntax parameter = method.ParameterList.Parameters.First();
                string parameterType = parameter.Type.ToString();
                string parameterName = parameter.Identifier.Text;

                //Method
                res +=
@$"
        public {returnType} {methodName}({parameterType} {parameterName})
        {{
            // Dummy implementation
        }}";
            }

            //End
            res +=
@$"
    }}
}}
";

            return res;
        }
    }
}
