using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace VenusRootLoader.Bootstrap.Settings;

public class LoggingSettings
{
    [Required]
    public bool? ShowConsole { get; set; }

    [Required]
    public bool? DisableUnityLogs { get; set; }

    [Required]
    public bool? EnableDiskLogging { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? DiskLoggingMaxFiles { get; set; }
}

[OptionsValidator]
public partial class ValidateLoggingSettings : IValidateOptions<LoggingSettings>;
