using Microsoft.Extensions.Configuration;
using VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

namespace VenusRootLoader.Bootstrap.Extensions;

public static class ConfigurationExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IConfigurationBuilder AddCustomEnvironmentVariables(
        this IConfigurationBuilder builder,
        string prefix,
        IDictionary<string, string> environmentVariablesMapping)
    {
        builder.Add(new CustomEnvironmentVariablesConfigurationSource
        {
            Prefix = prefix,
            EnvironmentVariablesMapping = environmentVariablesMapping
        });
        return builder;
    }
}