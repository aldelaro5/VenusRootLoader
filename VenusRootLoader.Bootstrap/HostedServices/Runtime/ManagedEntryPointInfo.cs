using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace VenusRootLoader.Bootstrap.HostedServices.Runtime;

public class ManagedEntryPointInfo
{
    [Required]
    public required string AssemblyPath { get; set; }
    [Required]
    public required string Namespace { get; set; }
    [Required]
    public required string ClassName { get; set; }
    [Required]
    public required string MethodName { get; set; }
}

[OptionsValidator]
public partial class ValidateManagedEntryPointInfoOptions : IValidateOptions<ManagedEntryPointInfo>;