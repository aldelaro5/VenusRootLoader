namespace VenusRootLoader;

public sealed class ModLoaderContext
{
    public required string ModsPath { get; init; }
    public required string ConfigPath { get; init; }
    public required string LoaderPath { get; init; }
}