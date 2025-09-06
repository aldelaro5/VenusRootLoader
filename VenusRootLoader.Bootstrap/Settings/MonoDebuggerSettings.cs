using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace VenusRootLoader.Bootstrap.Settings;

public class MonoDebuggerSettings
{
    [Required]
    public bool? Enable { get; set; }

    [Required]
    public required string IpAddress { get; set; }

    [Required]
    [Range(0, ushort.MaxValue)]
    public int Port { get; set; }

    [Required]
    public bool? SuspendOnBoot { get; set; }
}

[OptionsValidator]
public partial class ValidateMonoDebuggerSettings : IValidateOptions<MonoDebuggerSettings>;