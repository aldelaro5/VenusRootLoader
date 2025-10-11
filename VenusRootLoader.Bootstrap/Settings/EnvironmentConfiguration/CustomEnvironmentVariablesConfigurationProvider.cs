using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Collections;

namespace VenusRootLoader.Bootstrap.Settings.EnvironmentConfiguration;

/// <summary>
/// This is essentially a modified version of the <see cref="EnvironmentVariablesConfigurationProvider"/> that allows
/// to have custom mappings between the name of the variable and its configuration key. We use it to have the variable
/// have simpler names than the default.
/// </summary>
public class CustomEnvironmentVariablesConfigurationProvider : ConfigurationProvider
{
    private readonly string _prefix;
    private readonly IDictionary<string, string> _environmentVariablesMapping;

    public CustomEnvironmentVariablesConfigurationProvider(
        string prefix,
        IDictionary<string, string> environmentVariablesMapping)
    {
        _prefix = prefix;
        _environmentVariablesMapping = environmentVariablesMapping;
    }

    public override void Load() => Load(Environment.GetEnvironmentVariables());

    private void Load(IDictionary envVariables)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        IDictionaryEnumerator e = envVariables.GetEnumerator();
        try
        {
            while (e.MoveNext())
            {
                var key = (string)e.Entry.Key;
                var value = (string?)e.Entry.Value;

                if (!key.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                key = key.Substring(_prefix.Length);
                if (!_environmentVariablesMapping.TryGetValue(key, out var configKey))
                    continue;

                data[configKey] = value;
            }
        }
        finally
        {
            (e as IDisposable)?.Dispose();
        }

        Data = data;
    }
}