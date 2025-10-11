using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

// ReSharper disable UnusedMember.Global
namespace VenusRootLoader.Bootstrap.Mono;

/// <summary>
/// This interface contains abstractions to call Mono's functions. An instance is initialised by <see cref="MonoInitializer"/>
/// </summary>
public interface IMonoFunctions
{
    enum MonoDebugFormat
    {
        MonoDebugFormatNone,
        MonoDebugFormatMono,
        MonoDebugFormatDebugger
    }

    void Initialize(HMODULE handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate nint JitInitVersionFn(nint namePtr, nint runtimeVersionPtr);

    JitInitVersionFn JitInitVersion { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void JitParseOptionsFn(nint argc, string[] argv);

    JitParseOptionsFn JitParseOptions { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate nint ThreadCurrentFn();

    ThreadCurrentFn ThreadCurrent { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void DebugInitFn(MonoDebugFormat format);

    DebugInitFn DebugInit { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate void ConfigParseFn(string? configPath);

    ConfigParseFn ConfigParse { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void ThreadSetMainFn(nint thread);

    ThreadSetMainFn ThreadSetMain { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate nint RuntimeInvokeFn(nint method, nint obj, void** args, ref nint ex);

    RuntimeInvokeFn RuntimeInvoke { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void SetAssembliesPathFn(string domain);

    SetAssembliesPathFn SetAssembliesPath { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate string AssemblyGetrootdirFn();

    AssemblyGetrootdirFn AssemblyGetrootdir { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate nint DomainAssemblyOpenFn(nint domain, string path);

    DomainAssemblyOpenFn DomainAssemblyOpen { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate nint AssemblyGetImageFn(nint assembly);

    AssemblyGetImageFn AssemblyGetImage { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate nint ClassFromNameFn(nint image, string nameSpace, string name);

    ClassFromNameFn ClassFromName { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate nint ClassGetMethodFromNameFn(nint clas, string name, int paramCount);

    ClassGetMethodFromNameFn ClassGetMethodFromName { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate void DomainSetConfigFn(nint domain, string configPath, string configFile);

    DomainSetConfigFn DomainSetConfig { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate bool DebugEnabledFn();

    DebugEnabledFn DebugEnabled { get; }
}