using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace VenusRootLoader.Analyzers.Tests;

public sealed class BudClassAnalyzerTests
{
    [Fact]
    public async Task BudClassAnalyzer_EmitsDiagnostic_WhenNoBudClassesExists()
    {
        string text =
            """
            using VenusRootLoader.Api;

            public class SomeNonBud
            {
                protected virtual void Main() {}
            }

            public class SomeInheritedNonBud : SomeNonBud
            {
                protected override void Main() {}
            }

            public abstract class SomeAbstractNonBud : Bud
            {
            }

            public static class SomeStaticNonBud
            {
            }

            public interface IInterface
            {
                void Main();
            }
            """;

        await new CSharpAnalyzerTest<BudClassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
            TestState =
            {
                Sources = { text },
                AdditionalReferences = { "VenusRootLoader.dll" },
                ExpectedDiagnostics = { new(Descriptors.Vrl0001NoBudClass) },
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task BudClassAnalyzer_EmitsDiagnostic_WhenTwoBudClassesExists()
    {
        string text =
            $$"""
              using VenusRootLoader.Api;

              public class {|{{Descriptors.Vrl0002MoreThanOneBudClass.Id}}:SomeBud1|} : Bud
              {
                  protected override void Main() {}
              }

              public class {|{{Descriptors.Vrl0002MoreThanOneBudClass.Id}}:SomeBud2|} : Bud
              {
                  protected override void Main() {}
              }
              """;

        await new CSharpAnalyzerTest<BudClassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
            TestState =
            {
                Sources = { text },
                AdditionalReferences = { "VenusRootLoader.dll" }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task BudClassAnalyzer_EmitsNoDiagnostic_WhenOneBudClassesExists()
    {
        const string text =
            """
            using VenusRootLoader.Api;

            public class SomeBud : Bud
            {
                protected override void Main() {}
            }
            """;

        await new CSharpAnalyzerTest<BudClassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
            TestState =
            {
                Sources = { text },
                AdditionalReferences = { "VenusRootLoader.dll" }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task BudClassAnalyzer_EmitsNoDiagnostic_WhenOneBudClassesExistsAcrossMultiplePartials()
    {
        const string text =
            """
            using VenusRootLoader.Api;

            public partial class SomeBud : Bud
            {
                protected override void Main() {}
            }

            public partial class SomeBud
            {
                public void SomeOtherMethod() {}
            }
            """;

        await new CSharpAnalyzerTest<BudClassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Default,
            TestState =
            {
                Sources = { text },
                AdditionalReferences = { "VenusRootLoader.dll" }
            }
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}