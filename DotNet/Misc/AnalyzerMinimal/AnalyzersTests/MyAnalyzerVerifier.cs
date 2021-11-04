using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace AnalyzersTests
{
    public static class MyAnalyzerVerifier<TAnalyzer>
       where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            return CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);
        }

        public static async Task VerifyAnalyzerAsync(
           string source,
           params DiagnosticResult[] expected)
        {
            MyAnalyzerTest test = new(source, expected);
            await test.RunAsync(CancellationToken.None);
        }

        private class MyAnalyzerTest : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            public MyAnalyzerTest(
               string source,
               params DiagnosticResult[] expected)
            {
                TestCode = source;
                ExpectedDiagnostics.AddRange(expected);
                TestState.AdditionalReferences.Add(typeof(SomeAssembly1.SomeType1).Assembly);
                TestState.AdditionalReferences.Add(typeof(SomeAssembly2.SomeType2).Assembly);
            }
        }
    }
}
