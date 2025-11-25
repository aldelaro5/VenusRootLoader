using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace VenusRootLoader.Bootstrap.Settings;

public sealed class GlobalSettings
{
    [Required]
    public bool? DisableVrl { get; set; }

    [Required]
    public bool? SkipUnitySplashScreen { get; set; }
}

[OptionsValidator]
public sealed partial class ValidateGlobalSettings : IValidateOptions<GlobalSettings>;