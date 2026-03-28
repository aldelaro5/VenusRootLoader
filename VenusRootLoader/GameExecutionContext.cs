using System.Runtime.InteropServices;

namespace VenusRootLoader;

/// <summary>
/// Contains global information about the game's execution which is marshaled from the bootstrap.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal sealed class GameExecutionContext
{
    /// <summary>
    /// The directory the game resides in.
    /// </summary>
    public required string GameDir { get; init; }

    /// <summary>
    /// The data directory of the game that resides inside the <see cref="GameDir"/> directory.
    /// </summary>
    public required string DataDir { get; init; }

    /// <summary>
    /// The full path to the UnityPlayer.dll file of the game that resides inside the <see cref="GameDir"/> directory.
    /// </summary>
    public required string UnityPlayerDllFileName { get; init; }

    /// <summary>
    /// Tells if the game is executing under Wine or a Wine derived runtime such as Proton.
    /// </summary>
    public required bool IsWine { get; init; }
}