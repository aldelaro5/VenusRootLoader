using Microsoft.Extensions.Configuration;

namespace VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

public class CustomEnvironmentVariablesConfigurationSource : IConfigurationSource
{
    public required string Prefix { get; init; }
    public required IDictionary<string, string> EnvironmentVariablesMapping { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new CustomEnvironmentVariablesConfigurationProvider(Prefix, EnvironmentVariablesMapping);
    }
}