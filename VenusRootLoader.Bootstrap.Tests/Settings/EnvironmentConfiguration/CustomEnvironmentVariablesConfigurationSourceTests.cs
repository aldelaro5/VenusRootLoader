using System.Runtime.CompilerServices;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

namespace VenusRootLoader.Bootstrap.Tests.Settings.EnvironmentConfiguration;

public class CustomEnvironmentVariablesConfigurationSourceTests
{
    private readonly IConfigurationBuilder _configuration = Substitute.For<IConfigurationBuilder>();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_prefix")]
    private static extern ref string CustomEnvVarConfigProviderPrefix(CustomEnvironmentVariablesConfigurationProvider provider);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_environmentVariablesMapping")]
    private static extern ref IDictionary<string, string> CustomEnvVarConfigProviderMappings(CustomEnvironmentVariablesConfigurationProvider provider);

    [Fact]
    public void Build_GivesCorrectBuilder_WhenCalled()
    {
        var sut = new CustomEnvironmentVariablesConfigurationSource
        {
            Prefix = "PREFIX_",
            EnvironmentVariablesMapping = new Dictionary<string, string>
            {
                ["a"] = "b"
            }
        };
        var result = sut.Build(_configuration);

        result.Should().BeOfType<CustomEnvironmentVariablesConfigurationProvider>();
        CustomEnvVarConfigProviderPrefix((CustomEnvironmentVariablesConfigurationProvider)result)
            .Should().Be(sut.Prefix);
        CustomEnvVarConfigProviderMappings((CustomEnvironmentVariablesConfigurationProvider)result)
            .Should().BeSameAs(sut.EnvironmentVariablesMapping);
    }
}