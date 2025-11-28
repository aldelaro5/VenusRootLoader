using Microsoft.CodeAnalysis;

namespace VenusRootLoader.Analyzers;

internal static class Descriptors
{
    private const string Category = "Mod";

    internal static readonly DiagnosticDescriptor Vrl0001NoModClass = new(
        "VRL0001",
        "No mod classes",
        $"There are no classes which derive from {SampleSemanticAnalyzer.ModClassName}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: $"All mod assemblies must have one class derive from {SampleSemanticAnalyzer.ModClassName}.");

    internal static readonly DiagnosticDescriptor Vrl0002MoreThanOneModClass = new(
        "VRL0002",
        "Multiple mod classes",
        $"There are multiple classes which derive from {SampleSemanticAnalyzer.ModClassName}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: $"Only one class per mod assembly can derive from {SampleSemanticAnalyzer.ModClassName}.");

    internal static readonly DiagnosticDescriptor Vrl0003ModClassIsNotSealed = new(
        "VRL0003",
        "Mod class is not sealed",
        "The mod class should be sealed",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: "Deriving the mod class isn't beneficial and it is thus recommended to seal it.");
}