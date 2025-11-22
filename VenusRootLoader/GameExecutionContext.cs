using System.Runtime.InteropServices;

namespace VenusRootLoader;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal class GameExecutionContext
{
    public required string GameDir { get; init; }
    public required string DataDir { get; init; }
    public required string UnityPlayerDllFileName { get; init; }
    public required bool IsWine { get; init; }
}