using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// An object that contains information about the game's execution. The information are
/// collected on <see cref="Entry"/>
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class GameExecutionContext
{
    public required string GameDir { get; init; }
    public required string DataDir { get; init; }
    public required string UnityPlayerDllFileName { get; init; }
    public required bool IsWine { get; init; }
}