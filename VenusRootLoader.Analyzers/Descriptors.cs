using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VenusRootLoader.Analyzers.CodeFixes")]

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

    internal static readonly DiagnosticDescriptor Vrl0003BudClassIsNotSealed = new(
        "VRL0003",
        "Bud class is not sealed",
        "The bud class should be sealed",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: "Deriving the bud class isn't beneficial and it is thus recommended to seal it.");
}