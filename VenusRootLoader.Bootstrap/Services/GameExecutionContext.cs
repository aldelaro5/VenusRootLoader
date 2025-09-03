namespace VenusRootLoader.Bootstrap.Services;

public class GameExecutionContext
{
    public required nint LibraryHandle { get; init; }
    public required string GameDir { get; init; }
    public required string DataDir { get; init; }
    public required string UnityPlayerDllFileName { get; init; }
    public required bool IsWine { get; init; }
}