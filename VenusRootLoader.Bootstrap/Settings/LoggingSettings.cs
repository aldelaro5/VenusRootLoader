using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace VenusRootLoader.Bootstrap.Settings;

public class LoggingSettings
{
    [Required]
    public bool? HideConsole { get; set; }

    [Required]
    public bool? DisableUnityLogs { get; set; }
}

[OptionsValidator]
public partial class ValidateLoggingSettings : IValidateOptions<LoggingSettings>;
