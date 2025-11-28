using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;

namespace VenusRootLoader.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SampleSemanticAnalyzer : DiagnosticAnalyzer
{
    internal static readonly string ModClassName = "VenusRootLoader.ModLoading.Mod";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        // ReSharper disable once UseCollectionExpression
        ImmutableArray.Create(
            Descriptors.Vrl0001NoModClass,
            Descriptors.Vrl0002MoreThanOneModClass,
            Descriptors.Vrl0003ModClassIsNotSealed);

    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        analysisContext.EnableConcurrentExecution();
        analysisContext.RegisterCompilationStartAction(AnalyzeOperation);
    }

    private void AnalyzeOperation(CompilationStartAnalysisContext startContext)
    {
        ConcurrentBag<INamedTypeSymbol> foundModClasses = new();
        startContext.RegisterSymbolAction(SymbolAction, SymbolKind.NamedType);
        startContext.RegisterCompilationEndAction(CompilationEndAction);
        return;

        void CompilationEndAction(CompilationAnalysisContext context)
        {
            if (foundModClasses.IsEmpty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.Vrl0001NoModClass, Location.None));
                return;
            }

            if (foundModClasses.Count > 1)
            {
                List<Location> locations = foundModClasses.SelectMany(c => c.Locations).ToList();
                Diagnostic diagnosticMoreThanOneModClass = Diagnostic.Create(
                    Descriptors.Vrl0002MoreThanOneModClass,
                    locations[0],
                    locations.Skip(1));
                context.ReportDiagnostic(diagnosticMoreThanOneModClass);
                return;
            }

            INamedTypeSymbol foundModClass = foundModClasses.Single();
            if (foundModClass.IsSealed)
                return;

            Diagnostic diagnostic = Diagnostic.Create(
                Descriptors.Vrl0003ModClassIsNotSealed,
                foundModClass.Locations[0],
                foundModClass.Locations.Skip(1));
            context.ReportDiagnostic(diagnostic);
        }

        void SymbolAction(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
                return;

            if (type.BaseType?.ToDisplayString() == ModClassName)
                foundModClasses.Add(type);
        }
    }
}