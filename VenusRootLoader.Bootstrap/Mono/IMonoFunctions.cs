using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

// ReSharper disable UnusedMember.Global
namespace VenusRootLoader.Bootstrap.Mono;

/// <summary>
/// This class contains abstractions to call Mono's functions. An instance is initialised by <see cref="MonoInitializer"/>
/// </summary>
public interface IMonoFunctions
{
    public enum MonoDebugFormat
    {
        MonoDebugFormatNone,
        MonoDebugFormatMono,
        MonoDebugFormatDebugger
    }

    public void Initialize(HMODULE handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint JitInitVersionFn(nint namePtr, nint runtimeVersionPtr);
    public JitInitVersionFn JitInitVersion { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void JitParseOptionsFn(nint argc, string[] argv);
    public JitParseOptionsFn JitParseOptions { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint ThreadCurrentFn();
    public ThreadCurrentFn ThreadCurrent { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugInitFn(MonoDebugFormat format);
    public DebugInitFn DebugInit { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void ConfigParseFn(string? configPath);
    public ConfigParseFn ConfigParse { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ThreadSetMainFn(nint thread);
    public ThreadSetMainFn ThreadSetMain { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate nint RuntimeInvokeFn(nint method, nint obj, void** args, ref nint ex);
    public RuntimeInvokeFn RuntimeInvoke { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetAssembliesPathFn(string domain);
    public SetAssembliesPathFn SetAssembliesPath { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate string AssemblyGetrootdirFn();
    public AssemblyGetrootdirFn AssemblyGetrootdir { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint DomainAssemblyOpenFn(nint domain, string path);
    public DomainAssemblyOpenFn DomainAssemblyOpen { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint AssemblyGetImageFn(nint assembly);
    public AssemblyGetImageFn AssemblyGetImage { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint ClassFromNameFn(nint image, string nameSpace, string name);
    public ClassFromNameFn ClassFromName { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate nint ClassGetMethodFromNameFn(nint clas, string name, int paramCount);
    public ClassGetMethodFromNameFn ClassGetMethodFromName { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void DomainSetConfigFn(nint domain, string configPath, string configFile);
    public DomainSetConfigFn DomainSetConfig { get; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DebugEnabledFn();
    public DebugEnabledFn DebugEnabled { get; }
}
