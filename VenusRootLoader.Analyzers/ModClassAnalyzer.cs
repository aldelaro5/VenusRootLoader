using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;

namespace VenusRootLoader.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ModClassAnalyzer : DiagnosticAnalyzer
{
    internal const string ModClassName = "VenusRootLoader.ModLoading.Mod";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        // ReSharper disable once UseCollectionExpression
        ImmutableArray.Create(
            Descriptors.Vrl0001NoModClass,
            Descriptors.Vrl0002MoreThanOneModClass,
            Descriptors.Vrl0003ModClassIsNotSealed);

    private static bool IsModClass(INamedTypeSymbol type) =>
        type is { TypeKind: TypeKind.Class, IsAbstract: false, IsStatic: false }
        && type.BaseType?.ToDisplayString() == ModClassName;

    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        analysisContext.EnableConcurrentExecution();

        analysisContext.RegisterSymbolAction(AnalyseSymbol, SymbolKind.NamedType);
        analysisContext.RegisterCompilationStartAction(CompilerStartAction);
    }

    private static void AnalyseSymbol(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
        if (!IsModClass(type) || type.IsSealed)
            return;

        Diagnostic diagnostic = Diagnostic.Create(
            Descriptors.Vrl0003ModClassIsNotSealed,
            type.Locations[0],
            type.Locations.Skip(1));
        context.ReportDiagnostic(diagnostic);
    }

    private static void CompilerStartAction(CompilationStartAnalysisContext startContext)
    {
        ConcurrentBag<INamedTypeSymbol> foundModClasses = new();
        startContext.RegisterSymbolAction(AnalyseSymbolFromCompilationStart, SymbolKind.NamedType);
        startContext.RegisterCompilationEndAction(CompilationEndAction);
        return;

        void AnalyseSymbolFromCompilationStart(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (!IsModClass(type))
                return;

            foundModClasses.Add(type);
        }

        void CompilationEndAction(CompilationAnalysisContext context)
        {
            if (foundModClasses.IsEmpty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.Vrl0001NoModClass, Location.None));
                return;
            }

            if (foundModClasses.Count == 1)
                return;

            foreach (INamedTypeSymbol foundModClass in foundModClasses)
            {
                Diagnostic diagnosticMoreThanOneModClass = Diagnostic.Create(
                    Descriptors.Vrl0002MoreThanOneModClass,
                    foundModClass.Locations[0],
                    foundModClass.Locations.Skip(1));
                context.ReportDiagnostic(diagnosticMoreThanOneModClass);
            }
        }
    }
}