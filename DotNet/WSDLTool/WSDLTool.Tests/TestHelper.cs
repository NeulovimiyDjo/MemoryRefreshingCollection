using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnitTests.WSDLTool
{
    public static class TestHelper
    {
        private static List<PortableExecutableReference> _asmRefs = new string[]
        {
            "System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e",
            "System.Runtime, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
            "System.Private.ServiceModel, Version=4.8.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Runtime.Serialization.Primitives, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Runtime.Serialization.Xml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Private.DataContractSerialization, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        }.Select(asmName =>
        {
            Assembly asm = Assembly.Load(asmName);
            return MetadataReference.CreateFromFile(asm.Location);
        }).ToList();

        public static void CreateAssemblyFromSources(string outPath, params string[] sourceCodes)
        {
            IEnumerable<SyntaxTree> syntaxTrees = sourceCodes.Select(x => CSharpSyntaxTree.ParseText(x));

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(outPath),
                syntaxTrees,
                _asmRefs,
                options);

            using Stream dllStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var emitResult = compilation.Emit(dllStream);
            if (!emitResult.Success)
            {
                string errorMsg = emitResult.Diagnostics
                    .Select(d => d.ToString())
                    .Aggregate((d1, d2) => $"{d1}{Environment.NewLine}{d2}");

                throw new InvalidOperationException($"Errors!{Environment.NewLine}{errorMsg}");
            }
        }

        public static string CreateServiceDummyImplementationCode(string generatedServiceInterfaceContent)
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

        public static void RunWSDLTool(string arg)
        {
            string workDir = Directory.GetCurrentDirectory();
            ProcessStartInfo startInfo = new();
            startInfo.FileName = Path.Combine(workDir, "WSDLTool.exe");
            startInfo.WorkingDirectory = workDir;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = arg;
            startInfo.RedirectStandardError = true;
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            string errors = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errors))
                throw new InvalidOperationException(errors);
        }
    }
}
