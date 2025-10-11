using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace VenusRootLoader.Bootstrap.Settings;

public class GlobalSettings
{
    [Required]
    public bool? DisableVrl { get; set; }

    [Required]
    public bool? SkipUnitySplashScreen { get; set; }
}

[OptionsValidator]
public partial class ValidateGlobalSettings : IValidateOptions<GlobalSettings>;