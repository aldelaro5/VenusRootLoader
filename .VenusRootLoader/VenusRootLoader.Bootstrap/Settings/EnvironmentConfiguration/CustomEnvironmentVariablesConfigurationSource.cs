using Microsoft.Extensions.Configuration;

namespace VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

/// <summary>
/// This only exists to get a <see cref="CustomEnvironmentVariablesConfigurationProvider"/> to use
/// for the configuration system
/// </summary>
public sealed class CustomEnvironmentVariablesConfigurationSource : IConfigurationSource
{
    public required string Prefix { get; init; }
    public required IDictionary<string, string> EnvironmentVariablesMapping { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new CustomEnvironmentVariablesConfigurationProvider(Prefix, EnvironmentVariablesMapping);
    }
}