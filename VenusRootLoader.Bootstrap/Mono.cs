using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.HostedServices;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains abstractions to call Mono's functions. A static instance is initialised by <see cref="MonoInitializer"/>
/// </summary>
internal class Mono
{
    public enum MonoDebugFormat
    {
        MonoDebugFormatNone,
        MonoDebugFormatMono,
        MonoDebugFormatDebugger
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint JitInitVersionFn(nint namePtr, nint runtimeVersionPtr);
    public required JitInitVersionFn JitInitVersion { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void JitParseOptionsFn(nint argc, string[] argv);
    public required JitParseOptionsFn JitParseOptions { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint ThreadCurrentFn();
    public required ThreadCurrentFn ThreadCurrent { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugInitFn(MonoDebugFormat format);
    public required DebugInitFn DebugInit { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void ConfigParseFn(string? configPath);
    public required ConfigParseFn ConfigParse { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ThreadSetMainFn(nint thread);
    public required ThreadSetMainFn ThreadSetMain { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate nint RuntimeInvokeFn(nint method, nint obj, void** args, ref nint ex);
    public required RuntimeInvokeFn RuntimeInvoke { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint SetAssembliesPathFn(string domain);
    public required SetAssembliesPathFn SetAssembliesPath { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate string AssemblyGetrootdirFn();
    public required AssemblyGetrootdirFn AssemblyGetrootdir { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint DomainAssemblyOpenFn(nint domain, string path);
    public required DomainAssemblyOpenFn DomainAssemblyOpen { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint AssemblyGetImageFn(nint assembly);
    public required AssemblyGetImageFn AssemblyGetImage { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint ClassFromNameFn(nint image, string nameSpace, string name);
    public required ClassFromNameFn ClassFromName { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint ClassGetMethodFromNameFn(nint clas, string name, int paramCount);
    public required ClassGetMethodFromNameFn ClassGetMethodFromName { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void DomainSetConfigFn(nint domain, string configPath, string configFile);
    public required DomainSetConfigFn DomainSetConfig { get; init; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DebugEnabledFn();
    public required DebugEnabledFn DebugEnabled { get; init; }
}
