using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

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
