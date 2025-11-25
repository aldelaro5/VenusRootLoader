using System.ComponentModel.DataAnnotations;

namespace VenusRootLoader.Bootstrap.Settings.LogProvider;

public sealed class DiskFileLoggerSettings : ILogProviderSettings
{
    [Required]
    public bool? Enable { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? MaxFilesToKeep { get; set; }
}