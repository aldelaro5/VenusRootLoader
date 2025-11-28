using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VenusRootLoader.Analyzers.SampleSemanticAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VenusRootLoader.Analyzers.Tests;

public sealed class SampleSemanticAnalyzerTests
{
    [Fact]
    public async Task SetSpeedHugeSpeedSpecified_AlertDiagnostic()
    {
        const string text =
            """
            using VenusRootLoader.ModLoading;

            public class Spaceship : Mod
            {
                protected override void Main() {}
            }
            """;

        await new CSharpAnalyzerTest<SampleSemanticAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources = { text },
                AdditionalReferences = { "VenusRootLoader.dll" },
                ExpectedDiagnostics = { Verifier.Diagnostic() }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}