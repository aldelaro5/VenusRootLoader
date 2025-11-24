namespace VenusRootLoader.Build.Tasks;

public class ModManifest
{
    public required string AssemblyName { get; set; }
    public required string ModId { get; set; }
    public required string ModName { get; set; }
    public required Version ModVersion { get; set; }
    public required string ModAuthor { get; set; }
    public required ModDependency[] ModDependencies { get; set; }
    public required ModIncompatibility[] ModIncompatibilities { get; set; }
}

public class ModDependency
{
    public required string ModId { get; set; }
    public required bool Optional { get; set; }
}

public class ModIncompatibility
{
    public required string ModId { get; set; }
}
