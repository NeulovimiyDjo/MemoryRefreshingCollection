using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyA = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Analyzer.MyAnalyzer>;
using VerifyCF = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<Analyzer.MyAnalyzer, Analyzer.MyAnalyzerCodeFixProvider>;

namespace AnalyzersTests
{
    public class MyAnalyzerTests
    {
        [Fact]
        public async Task MyAnalyzerOnlyTest()
        {
            string testCode =
@"class ClassName1
{
    public int Method1(int p1, int p2)
    {
        return p1 + p2;
    }
}";

            DiagnosticResult expected = DiagnosticResult
                .CompilerWarning("DA_01")
                .WithSpan(1, 7, 1, 17)
                .WithArguments("ClassName1")
                .WithMessage("Type name is lower case");
            await VerifyA.VerifyAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task MyAnalyzerWithCodeFixTest()
        {
            string testCode =
@"class {|#123:ClassName1|}
{
    public int Method1(int p1, int p2)
    {
        return p1 + p2;
    }
}";

            string fixedCode =
@"class CLASSNAME1
{
    public int Method1(int p1, int p2)
    {
        return p1 + p2;
    }
}";

            DiagnosticResult expected = DiagnosticResult
                .CompilerWarning("DA_01")
                .WithLocation(123)
                .WithArguments("ClassName1")
                .WithMessage("Type name is lower case");
            await VerifyCF.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
