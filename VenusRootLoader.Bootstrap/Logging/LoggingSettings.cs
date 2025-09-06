using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace VenusRootLoader.Bootstrap.Logging;

public class LoggingSettings
{
    [Required]
    public bool? HideConsole { get; set; }
}

[OptionsValidator]
public partial class ValidateLoggingSettings : IValidateOptions<LoggingSettings>;

