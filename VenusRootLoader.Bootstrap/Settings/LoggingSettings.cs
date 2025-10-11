using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using VenusRootLoader.Bootstrap.Settings.LogProvider;

namespace VenusRootLoader.Bootstrap.Settings;

public class LoggingSettings
{
    [Required]
    public bool? IncludeUnityLogs { get; set; }

    [ValidateObjectMembers]
    [Required]
    public required ConsoleLoggerSettings ConsoleLoggerSettings { get; set; }

    [ValidateObjectMembers]
    [Required]
    public required DiskFileLoggerSettings DiskFileLoggerSettings { get; set; }
}

[OptionsValidator]
public partial class ValidateLoggingSettings : IValidateOptions<LoggingSettings>;