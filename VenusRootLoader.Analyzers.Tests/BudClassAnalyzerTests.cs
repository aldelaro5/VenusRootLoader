using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VenusRootLoader.Analyzers.BudClassAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VenusRootLoader.Analyzers.Tests;

public sealed class BudClassAnalyzerTests
{
    [Fact]
    public async Task SetSpeedHugeSpeedSpecified_AlertDiagnostic()
    {
        const string text =
            """
            using VenusRootLoader.BudLoading;

            public class Spaceship : Bud
            {
                protected override void Main() {}
            }
            """;

        await new CSharpAnalyzerTest<BudClassAnalyzer, DefaultVerifier>
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