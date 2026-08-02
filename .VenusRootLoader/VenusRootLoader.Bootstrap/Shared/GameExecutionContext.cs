using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// An object that contains information about the game's execution. The information are
/// collected on <see cref="Entry"/>
/// </summary>
public interface IGameExecutionContext
{
    string GameDir { get; init; }
    string DataDir { get; init; }
    string UnityPlayerDllFileName { get; init; }
    bool IsWine { get; init; }
    nint GetPointer();
}

/// <inheritdoc/>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public sealed class GameExecutionContext : IGameExecutionContext
{
    public required string GameDir { get; init; }
    public required string DataDir { get; init; }
    public required string UnityPlayerDllFileName { get; init; }
    public required bool IsWine { get; init; }

    public nint GetPointer()
    {
        nint gameExecutionContextPtr = Marshal.AllocHGlobal(Marshal.SizeOf<GameExecutionContext>());
        Marshal.StructureToPtr(this, gameExecutionContextPtr, false);
        return gameExecutionContextPtr;
    }
}