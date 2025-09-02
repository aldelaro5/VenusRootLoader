namespace VenusRootLoader.Bootstrap;

internal class GameExecutionContext
{
    internal required nint LibraryHandle { get; init; }
    internal required string GameDir { get; init; }
    internal required string DataDir { get; init; }
    internal required string UnityPlayerDllFileName { get; init; }
}