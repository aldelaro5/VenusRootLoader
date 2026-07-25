using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VenusRootLoader.Analyzers.Tests")]

namespace VenusRootLoader.Analyzers;

internal static class Descriptors
{
    private const string Category = "Bud";

    internal static readonly DiagnosticDescriptor Vrl0001NoBudClass = new(
        "VRL0001",
        "No bud classes",
        $"There are no classes which derive from {BudClassAnalyzer.BudClassName}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: $"All bud assemblies must have one class derive from {BudClassAnalyzer.BudClassName}.");

    internal static readonly DiagnosticDescriptor Vrl0002MoreThanOneBudClass = new(
        "VRL0002",
        "Multiple bud classes",
        $"There are multiple classes which derive from {BudClassAnalyzer.BudClassName}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: $"Only one class per bud assembly can derive from {BudClassAnalyzer.BudClassName}.");
}