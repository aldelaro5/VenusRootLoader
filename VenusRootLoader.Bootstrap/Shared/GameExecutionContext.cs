namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// An object that contains information about the game's execution. The information are
/// collected on <see cref="Entry"/>
/// </summary>
public class GameExecutionContext
{
    public required nint LibraryHandle { get; init; }
    public required string GameDir { get; init; }
    public required string DataDir { get; init; }
    public required string UnityPlayerDllFileName { get; init; }
    public required bool IsWine { get; init; }
}