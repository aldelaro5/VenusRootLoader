using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

namespace VenusRootLoader.Bootstrap.Tests.Settings.EnvironmentConfiguration;

public sealed class CustomEnvironmentVariablesConfigurationProviderTests
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Data")]
    private static extern IDictionary<string, string?> CustomEnvVarConfigProviderData(ConfigurationProvider provider);

    [Fact]
    public void Load_SetCorrectConfigData_WhenCalled()
    {
        var prefix = "PREFIX_";
        var configKey = "ConfigKey";
        var value = "true";
        var mappings = new Dictionary<string, string> { ["EXIST"] = configKey };
        var sut = new CustomEnvironmentVariablesConfigurationProvider(prefix, mappings);
        Environment.SetEnvironmentVariable("NO_PREFIX", value);
        Environment.SetEnvironmentVariable($"{prefix}DOES_NOT_EXIST", value);
        Environment.SetEnvironmentVariable($"{prefix}EXIST", value);
        sut.Load();

        CustomEnvVarConfigProviderData(sut)
            .Should().ContainSingle(kvp => kvp.Key == configKey && kvp.Value == value);
    }
}