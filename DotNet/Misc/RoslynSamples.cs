	
	
        private static Assembly CompileInMemoryAnLoad(Compilation compilation, IEnumerable<ResourceDescription> resources = null)
        {
            using MemoryStream pdbStream = new();
            using MemoryStream assemblyStream = new();
            EmitResult result = compilation.Emit(assemblyStream, pdbStream: pdbStream, manifestResources: resources);
            if (!result.Success)
                throw new Exception(GetCompilationErrorString(result));
            byte[] assemblyBytes = assemblyStream.ToArray();
            byte[] pdbBytes = pdbStream.ToArray();
            Assembly assembly = Assembly.Load(assemblyBytes, pdbBytes);
            return assembly;
        }

        private static string GetCompilationErrorString(EmitResult result)
        {
            DiagnosticFormatter formatter = new();
            List<string> errors = new(result.Diagnostics.Select(d => formatter.Format(d)));
            return string.Join("\n", errors);
        }
	
	
	
        public static bool ContainsClassImplementingInterface(Compilation compilation)
        {
            //System.Diagnostics.Debugger.Launch();
            foreach (SyntaxTree st in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(st);
                IEnumerable<ClassDeclarationSyntax> classes = st.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();
                if (classes.Any())
                {
                    ClassDeclarationSyntax c = classes.First();
                    ITypeSymbol classSymbol = (ITypeSymbol)semanticModel.GetDeclaredSymbol(c);
                    if (classSymbol.Interfaces.First().Name == "ISomeInterface" &&
                    classSymbol.Interfaces.First().ContainingAssembly.Name == "SomeAssembly.SomeSubnamespace")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ProcessClassFieldsOrProperties(Compilation compilation)
        {
            //System.Diagnostics.Debugger.Launch();
            foreach (SyntaxTree st in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(st);
                IEnumerable<ClassDeclarationSyntax> classes = st.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();
                foreach (ClassDeclarationSyntax c in classes)
                {
                    ITypeSymbol classSymbol = (ITypeSymbol)semanticModel.GetDeclaredSymbol(c);
                    if (classSymbol.Interfaces.First().Name != nameof(ISomeInterface))
                        continue;
                    List<string> listOfFieldsOrProperties = classSymbol.GetMembers()
                        .Where(x =>
                            x.Kind == SymbolKind.Field &&
                                ((IFieldSymbol)x).Type.ContainingAssembly == "SomeAssembly.SomeSubnamespace"
									&& ((IFieldSymbol)x).Type.Name == "SomeType" ||
                            x.Kind == SymbolKind.Property &&
                                ((IPropertySymbol)x).Type.ToString() == "SomeAssembly.SomeSubnamespace.SomeType")
                        .Select(x => x.Name )
                        .ToList();
                }
            }
        }
	
	
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
	}