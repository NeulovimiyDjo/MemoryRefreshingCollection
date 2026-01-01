using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MA_01";
        public const string Title = "Make type name upper case";

        private static readonly DiagnosticDescriptor s_diagnosticDescriptor = new(
            id: DiagnosticId,
            title: Title,
            messageFormat: "Type name is lower case",
            category: "Naming",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Found inappropriate type name (contains lowercase letters). Ensure type names are upper case.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_diagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                Diagnostic diagnostic = Diagnostic.Create(s_diagnosticDescriptor, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
