namespace VenusRootLoader.Api;

public sealed class BudLoaderContext
{
    public required string BudsPath { get; init; }
    public required string ConfigPath { get; init; }
    public required string LoaderPath { get; init; }
}