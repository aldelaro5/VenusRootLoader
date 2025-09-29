using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using VenusRootLoader.Bootstrap.Extensions;

namespace VenusRootLoader.Bootstrap.Mono;

public class MonoFunctions : IMonoFunctions
{
    public void Initialize(HMODULE handle)
    {
        RuntimeInvoke = NativeLibrary.GetExportDelegate<IMonoFunctions.RuntimeInvokeFn>(handle, "mono_runtime_invoke");
        JitInitVersion = NativeLibrary.GetExportDelegate<IMonoFunctions.JitInitVersionFn>(handle, "mono_jit_init_version");
        JitParseOptions = NativeLibrary.GetExportDelegate<IMonoFunctions.JitParseOptionsFn>(handle, "mono_jit_parse_options");
        ThreadCurrent = NativeLibrary.GetExportDelegate<IMonoFunctions.ThreadCurrentFn>(handle, "mono_thread_current");
        DebugEnabled = NativeLibrary.GetExportDelegate<IMonoFunctions.DebugEnabledFn>(handle, "mono_debug_enabled");
        DebugInit = NativeLibrary.GetExportDelegate<IMonoFunctions.DebugInitFn>(handle, "mono_debug_init");
        ThreadSetMain = NativeLibrary.GetExportDelegate<IMonoFunctions.ThreadSetMainFn>(handle, "mono_thread_set_main");
        SetAssembliesPath = NativeLibrary.GetExportDelegate<IMonoFunctions.SetAssembliesPathFn>(handle, "mono_set_assemblies_path");
        AssemblyGetrootdir = NativeLibrary.GetExportDelegate<IMonoFunctions.AssemblyGetrootdirFn>(handle, "mono_assembly_getrootdir");
        DomainAssemblyOpen = NativeLibrary.GetExportDelegate<IMonoFunctions.DomainAssemblyOpenFn>(handle, "mono_domain_assembly_open");
        AssemblyGetImage = NativeLibrary.GetExportDelegate<IMonoFunctions.AssemblyGetImageFn>(handle, "mono_assembly_get_image");
        ClassFromName = NativeLibrary.GetExportDelegate<IMonoFunctions.ClassFromNameFn>(handle, "mono_class_from_name");
        ClassGetMethodFromName = NativeLibrary.GetExportDelegate<IMonoFunctions.ClassGetMethodFromNameFn>(handle, "mono_class_get_method_from_name");
        DomainSetConfig = NativeLibrary.GetExportDelegate<IMonoFunctions.DomainSetConfigFn>(handle, "mono_domain_set_config");
        ConfigParse = NativeLibrary.GetExportDelegate<IMonoFunctions.ConfigParseFn>(handle, "mono_config_parse");
    }

    public IMonoFunctions.JitInitVersionFn JitInitVersion { get; private set; } = null!;
    public IMonoFunctions.JitParseOptionsFn JitParseOptions { get; private set; } = null!;
    public IMonoFunctions.ThreadCurrentFn ThreadCurrent { get; private set; } = null!;
    public IMonoFunctions.DebugInitFn DebugInit { get; private set; } = null!;
    public IMonoFunctions.ConfigParseFn ConfigParse { get; private set; } = null!;
    public IMonoFunctions.ThreadSetMainFn ThreadSetMain { get; private set; } = null!;
    public IMonoFunctions.RuntimeInvokeFn RuntimeInvoke { get; private set; } = null!;
    public IMonoFunctions.SetAssembliesPathFn SetAssembliesPath { get; private set; } = null!;
    public IMonoFunctions.AssemblyGetrootdirFn AssemblyGetrootdir { get; private set; } = null!;
    public IMonoFunctions.DomainAssemblyOpenFn DomainAssemblyOpen { get; private set; } = null!;
    public IMonoFunctions.AssemblyGetImageFn AssemblyGetImage { get; private set; } = null!;
    public IMonoFunctions.ClassFromNameFn ClassFromName { get; private set; } = null!;
    public IMonoFunctions.ClassGetMethodFromNameFn ClassGetMethodFromName { get; private set; } = null!;
    public IMonoFunctions.DomainSetConfigFn DomainSetConfig { get; private set; } = null!;
    public IMonoFunctions.DebugEnabledFn DebugEnabled { get; private set; } = null!;
}
