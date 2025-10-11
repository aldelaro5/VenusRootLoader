using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace VenusRootLoader.Bootstrap.Settings;

public class MonoDebuggerSettings
{
    [Required]
    public bool? Enable { get; set; }

    [Required]
    [RegularExpression(
        @"^(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
        ErrorMessage = "Must be a valid IPv4 address.")]
    public required string IpAddress { get; set; }

    [Required]
    [Range(0, ushort.MaxValue)]
    public int? Port { get; set; }

    [Required]
    public bool? SuspendOnBoot { get; set; }
}

[OptionsValidator]
public partial class ValidateMonoDebuggerSettings : IValidateOptions<MonoDebuggerSettings>;