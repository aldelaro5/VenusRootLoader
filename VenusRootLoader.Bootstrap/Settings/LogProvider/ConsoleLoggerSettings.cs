using System.ComponentModel.DataAnnotations;

namespace VenusRootLoader.Bootstrap.Settings.LogProvider;

public class ConsoleLoggerSettings : ILogProviderSettings
{
    [Required]
    public bool? Enable { get; set; }

    [Required]
    public bool? LogWithColors { get; set; }
}