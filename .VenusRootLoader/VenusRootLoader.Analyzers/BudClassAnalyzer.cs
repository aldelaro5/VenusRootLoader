using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;

namespace VenusRootLoader.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BudClassAnalyzer : DiagnosticAnalyzer
{
    internal const string BudClassName = "VenusRootLoader.Api.Bud";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        // ReSharper disable once UseCollectionExpression
        ImmutableArray.Create(
            Descriptors.Vrl0001NoBudClass,
            Descriptors.Vrl0002MoreThanOneBudClass);

    private static bool IsBudClass(INamedTypeSymbol type) =>
        type is { TypeKind: TypeKind.Class, IsAbstract: false, IsStatic: false }
        && type.BaseType?.ToDisplayString() == BudClassName;

    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        analysisContext.EnableConcurrentExecution();

        analysisContext.RegisterCompilationStartAction(CompilerStartAction);
    }

    private static void CompilerStartAction(CompilationStartAnalysisContext startContext)
    {
        ConcurrentBag<INamedTypeSymbol> foundBudClasses = new();
        startContext.RegisterSymbolAction(AnalyseSymbolFromCompilationStart, SymbolKind.NamedType);
        startContext.RegisterCompilationEndAction(CompilationEndAction);
        return;

        void AnalyseSymbolFromCompilationStart(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (!IsBudClass(type))
                return;

            foundBudClasses.Add(type);
        }

        void CompilationEndAction(CompilationAnalysisContext context)
        {
            if (foundBudClasses.IsEmpty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.Vrl0001NoBudClass, Location.None));
                return;
            }

            if (foundBudClasses.Count == 1)
                return;

            foreach (INamedTypeSymbol foundBudClass in foundBudClasses)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.Vrl0002MoreThanOneBudClass,
                        foundBudClass.Locations[0],
                        foundBudClass.Locations.Skip(1)));
            }
        }
    }
}