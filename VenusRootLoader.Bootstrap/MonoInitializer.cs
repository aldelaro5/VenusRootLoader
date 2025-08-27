using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class initialises an instance of <see cref="Mono"/>, initialises the Mono runtime using various hooks,
/// and transitions to the managed side using the newly initialised runtime
/// </summary>
public static class MonoInitializer
{
    public struct ManagedEntryPointInfo
    {
        public required string AssemblyPath { get; init; }
        public required string Namespace { get; init; }
        public required string ClassName { get; init; }
        public required string MethodName { get; init; }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate nint GetProcAddressFn(nint handle, string symbol);
    private static readonly GetProcAddressFn HookGetProcAddressDelegate = HookGetProcAddress;

    private static ManagedEntryPointInfo _managedEntryPointInfo;
    private static bool _runtimeInitialised;
    private static bool _debugInitCalled;
    private static bool _jitInitDone;

    private static nint Domain { get; set; }
    private static Mono Mono { get; set; } = null!;
    private static string _additionalMonoAssembliesPath = string.Empty;

    private const string MonoDebugArgsStart = "--debugger-agent=transport=dt_socket,server=y,address=";
    private const string MonoDebugNoSuspendArg = ",suspend=n";

    private static readonly Mono.JitInitVersionFn MonoInitDetourFn = MonoJitInitDetour;
    private static readonly Mono.JitParseOptionsFn JitParseOptionsDetourFn = MonoJitParseOptionsDetour;
    private static readonly Mono.DebugInitFn DebugInitDetourFn = MonoDebugInitDetour;
    private static readonly unsafe Mono.ImageOpenFromDataWithNameFn ImageOpenFromDataWithNameFn = MonoImageOpenFromDataWithNameDetour;
    private static readonly Dictionary<string, nint> SymbolRedirects = new()
    {
        { "mono_jit_init_version", Marshal.GetFunctionPointerForDelegate(MonoInitDetourFn)},
        { "mono_jit_parse_options", Marshal.GetFunctionPointerForDelegate(JitParseOptionsDetourFn)},
        { "mono_debug_init", Marshal.GetFunctionPointerForDelegate(DebugInitDetourFn)},
        { "mono_image_open_from_data_with_name", Marshal.GetFunctionPointerForDelegate(ImageOpenFromDataWithNameFn)}
    };

    public static void Setup(ManagedEntryPointInfo entryPointInfo)
    {
        // if (!File.Exists(entryPointInfo.AssemblyPath))
        // {
        //     Console.WriteLine($"Could not find the entrypoint assembly {entryPointInfo.AssemblyPath}");
        //     return;
        // }

        _managedEntryPointInfo = entryPointInfo;
        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "GetProcAddress", Marshal.GetFunctionPointerForDelegate(HookGetProcAddressDelegate));
    }

    // Unity obtains Mono's symbols by calling GetProcAddress on them as opposed to calling them via their imports.
    // This means we can't PltHook them directly, but we can PltHook GetProcAddress and redirect the ones we're
    // interested in which is what this hook does. It also gives us the Mono's handle as an added bonus since we need it
    private static nint HookGetProcAddress(nint handle, string symbol)
    {
        nint originalSymbolAddress = WindowsNative.GetProcAddress(handle, symbol);
        if (!SymbolRedirects.TryGetValue(symbol, out var detourAddress))
            return originalSymbolAddress;

        if (!_runtimeInitialised)
            RetrieveMonoExports(handle);
        _runtimeInitialised = true;

        Console.WriteLine($"Redirecting {symbol}");
        return detourAddress;
    }

    private static void RetrieveMonoExports(nint handle)
    {
        Console.WriteLine("Loading Mono exports");
        Mono = new Mono
        {
            Handle = handle,
            RuntimeInvoke = NativeLibrary.GetExportDelegate<Mono.RuntimeInvokeFn>(handle, "mono_runtime_invoke"),
            JitInitVersion = NativeLibrary.GetExportDelegate<Mono.JitInitVersionFn>(handle, "mono_jit_init_version"),
            JitParseOptions = NativeLibrary.GetExportDelegate<Mono.JitParseOptionsFn>(handle, "mono_jit_parse_options"),
            ThreadCurrent = NativeLibrary.GetExportDelegate<Mono.ThreadCurrentFn>(handle, "mono_thread_current"),
            DebugEnabled = NativeLibrary.GetExportDelegate<Mono.DebugEnabledFn>(handle, "mono_debug_enabled"),
            DebugInit = NativeLibrary.GetExportDelegate<Mono.DebugInitFn>(handle, "mono_debug_init"),
            ThreadSetMain = NativeLibrary.GetExportDelegate<Mono.ThreadSetMainFn>(handle, "mono_thread_set_main"),
            StringNew = NativeLibrary.GetExportDelegate<Mono.StringNewFn>(handle, "mono_string_new"),
            AssemblyGetObject = NativeLibrary.GetExportDelegate<Mono.AssemblyGetObjectFn>(handle, "mono_assembly_get_object"),
            MethodGetName = NativeLibrary.GetExportDelegate<Mono.MethodGetNameFn>(handle, "mono_method_get_name"),
            SetAssembliesPath = NativeLibrary.GetExportDelegate<Mono.SetAssembliesPathFn>(handle, "mono_set_assemblies_path"),
            AssemblyGetrootdir = NativeLibrary.GetExportDelegate<Mono.AssemblyGetrootdirFn>(handle, "mono_assembly_getrootdir"),
            AddInternalCall = NativeLibrary.GetExportDelegate<Mono.AddInternalCallFn>(handle, "mono_add_internal_call"),
            DomainAssemblyOpen = NativeLibrary.GetExportDelegate<Mono.DomainAssemblyOpenFn>(handle, "mono_domain_assembly_open"),
            AssemblyGetImage = NativeLibrary.GetExportDelegate<Mono.AssemblyGetImageFn>(handle, "mono_assembly_get_image"),
            ClassFromName = NativeLibrary.GetExportDelegate<Mono.ClassFromNameFn>(handle, "mono_class_from_name"),
            ClassGetMethodFromName = NativeLibrary.GetExportDelegate<Mono.ClassGetMethodFromNameFn>(handle, "mono_class_get_method_from_name"),
            ImageOpenFromDataWithName = NativeLibrary.GetExportDelegate<Mono.ImageOpenFromDataWithNameFn>(handle, "mono_image_open_from_data_with_name"),
            DomainSetConfig = NativeLibrary.GetExportDelegate<Mono.DomainSetConfigFn>(handle, "mono_domain_set_config"),
            ConfigParse = NativeLibrary.GetExportDelegate<Mono.ConfigParseFn>(handle, "mono_config_parse")
        };
    }

    // This hooks the main Mono initialization function so it contains most of the machinery needed.
    // Most of the initialization process replicates what Unity does so we're effectively seizing control of the runtime
    private static nint MonoJitInitDetour(nint domainName, nint runtimeVersion)
    {
        if (_jitInitDone)
            return Mono.JitInitVersion(domainName, runtimeVersion);

        Console.WriteLine("In init detour");
        string domainNameStr = Marshal.PtrToStringAnsi(domainName)!;
        string runtimeVersionStr = Marshal.PtrToStringAnsi(runtimeVersion)!;
        Console.WriteLine($"Domain: {domainNameStr}, Runtime version: {runtimeVersionStr}");

        SetMonoAssembliesPath();

        MonoJitParseOptionsDetour(0, []);
        InitialiseMonoDebuggerIfNeeded();

        Console.WriteLine("Original init jit version");
        Domain = Mono.JitInitVersion(domainName, runtimeVersion);

        SetMonoMainThreadToCurrentThread();
        SetupMonoConfigs();

        TransitionToMonoManagedSide();

        _jitInitDone = true;
        return Domain;
    }

    private static void SetupMonoConfigs()
    {
        string configFile = $"{Environment.ProcessPath}.config";
        Console.WriteLine($"Setting Mono Config paths: base_dir: {Entry.GameDir}, config_file_name: {configFile}");
        Mono.DomainSetConfig(Domain, Entry.GameDir, configFile);

        Console.WriteLine("Parsing default Mono config");
        Mono.ConfigParse(null);
    }

    private static void SetMonoMainThreadToCurrentThread()
    {
        Console.WriteLine("Setting Mono Main Thread");
        Mono.ThreadSetMain(Mono.ThreadCurrent());
    }

    private static void InitialiseMonoDebuggerIfNeeded()
    {
        bool debuggerAlreadyEnabled = _debugInitCalled || Mono.DebugEnabled();
        if (debuggerAlreadyEnabled)
            return;

        Console.WriteLine("Initialising Mono debugger");
        Mono.DebugInit(Mono.MonoDebugFormat.MonoDebugFormatMono);
    }

    private static void SetMonoAssembliesPath()
    {
        StringBuilder newAssembliesPathSb = new();
        newAssembliesPathSb.Append(Mono.AssemblyGetrootdir());
        _additionalMonoAssembliesPath = "";

        string newAssembliesPath = newAssembliesPathSb.ToString();
        Console.WriteLine($"Setting Mono assemblies path to: {newAssembliesPath}");
        Mono.SetAssembliesPath(newAssembliesPath);
    }

    // This hooks appends debugger arguments to enable Mono's SDB server
    private static void MonoJitParseOptionsDetour(nint argc, string[] argv)
    {
        if (_jitInitDone)
        {
            Mono.JitParseOptions(argc, argv);
            return;
        }
        Console.WriteLine("jit parse options");

        string newArgs;
        string? dnSpyEnv = Environment.GetEnvironmentVariable("DNSPY_UNITY_DBG2");
        if (dnSpyEnv is null)
        {
            StringBuilder newArgsSb = new(MonoDebugArgsStart);
            newArgsSb.Append("127.0.0.1");
            newArgsSb.Append(':');
            newArgsSb.Append("10000");
            if (true)
                newArgsSb.Append(MonoDebugNoSuspendArg);
            newArgs = newArgsSb.ToString();
        }
        else
        {
            newArgs = dnSpyEnv;
        }

        string[] newArgv = new string[argc + 1];
        Array.Copy(argv, 0, newArgv, 0, argc);
        argc++;
        newArgv[argc - 1] = newArgs;

        Console.WriteLine($"Adding jit option: {string.Join(' ', newArgs)}");

        Mono.JitParseOptions(argc, newArgv);
    }

    // It's possible that Mono already wants to initialise its SDB server. We don't have a problem with this, but we do
    // need to make sure that we don't initialise it twice so this hook is still needed to detect this
    private static void MonoDebugInitDetour(Mono.MonoDebugFormat format)
    {
        Console.WriteLine("Debug init");
        _debugInitCalled = true;
        Mono.DebugInit(format);
    }

    private static unsafe void TransitionToMonoManagedSide()
    {
        Console.WriteLine("Loading entrypoint assembly");
        var assembly = Mono.DomainAssemblyOpen(Domain, _managedEntryPointInfo.AssemblyPath);
        if (assembly == 0)
        {
            Console.WriteLine("Failed to load the entrypoint assembly into the Mono domain");
            return;
        }

        var image = Mono.AssemblyGetImage(assembly);
        var interopClass = Mono.ClassFromName(image, _managedEntryPointInfo.Namespace, _managedEntryPointInfo.ClassName);
        var initMethod = Mono.ClassGetMethodFromName(interopClass, _managedEntryPointInfo.MethodName, 0);

        nint ex = 0;
        var initArgs = stackalloc nint*[] { };
        Console.WriteLine("Invoking entrypoint method");
        Mono.RuntimeInvoke(initMethod, 0, (void**)initArgs, ref ex);
    }

    // Unity is sneaky, and it will call mono_image_open_from_data_with_name on assemblies Mono resolved beforehand.
    // This is a problem because it bypasses Mono's assemblies path which we modify to load the unstripped assemblies
    // containing the BCL. It effectively means it will load the assemblies from the game's Managed folder when we
    // specifically do NOT want them in the case of the BCL. This forces us to hook on this Mono function so it takes
    // the Mono's assemblies path into account so we load the assembly we want instead
    private static unsafe nint MonoImageOpenFromDataWithNameDetour(
        byte* data,
        uint dataLen,
        bool needCopy,
        ref Mono.MonoImageOpenStatus status,
        bool refOnly,
        string name)
    {
        if (string.IsNullOrEmpty(name))
            return Mono.ImageOpenFromDataWithName(data, dataLen, needCopy, ref status, refOnly, name);

        string fileName = Path.GetFileName(name);
        var foundOverridenFile = _additionalMonoAssembliesPath
            .Split(";")
            .Select(x => Path.GetFullPath(Path.Combine(x, fileName)))
            .FirstOrDefault(File.Exists);

        if (foundOverridenFile == null)
            return Mono.ImageOpenFromDataWithName(data, dataLen, needCopy, ref status, refOnly, name);

        Console.WriteLine($"Overriding the image load of {name} to {foundOverridenFile}");
        byte[] newDataArray = File.ReadAllBytes(foundOverridenFile);
        uint newDataLen = (uint)newDataArray.Length;
        fixed (byte* newDataPtr = &newDataArray[0])
        {
            var newReturn = Mono.ImageOpenFromDataWithName(newDataPtr, newDataLen, needCopy, ref status, refOnly, foundOverridenFile);
            return newReturn;
        }
    }
}
