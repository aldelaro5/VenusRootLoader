using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.Services;
using VenusRootLoader.Bootstrap.Settings;

namespace VenusRootLoader.Bootstrap.HostedServices.Runtime;

/// <summary>
/// This class initialises an instance of <see cref="Mono"/>, initialises the Mono runtime using various hooks,
/// and transitions to the managed side using the newly initialised runtime
/// </summary>
internal class MonoInitializer : IHostedService
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate nint GetProcAddressFn(HMODULE handle, PCSTR symbol);
    private static GetProcAddressFn _hookGetProcAddressDelegate = null!;

    private readonly ManagedEntryPointInfo _managedEntryPointInfo;
    private bool _runtimeInitialised;
    private bool _debugInitCalled;
    private bool _jitInitDone;

    private nint Domain { get; set; }
    private Mono Mono { get; set; } = null!;
    private string _additionalMonoAssembliesPath = string.Empty;

    private const string MonoDebugArgsStart = "--debugger-agent=transport=dt_socket,server=y,address=";
    private const string MonoDebugNoSuspendArg = ",suspend=n";

    private static Mono.JitInitVersionFn _monoInitDetourFn = null!;
    private static Mono.JitParseOptionsFn _jitParseOptionsDetourFn = null!;
    private static Mono.DebugInitFn _debugInitDetourFn = null!;

    private readonly Dictionary<string, nint> _symbolRedirects;
    
    private readonly PltHook _pltHook;
    private readonly ILogger _logger;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly MonoDebuggerSettings _debuggerSettings;

    public MonoInitializer(
        ILogger<MonoInitializer> logger,
        PltHook pltHook,
        GameExecutionContext gameExecutionContext,
        IOptions<ManagedEntryPointInfo> entryPointInfo, IOptions<MonoDebuggerSettings> debuggerSettings)
    {
        _logger = logger;
        _pltHook = pltHook;

        _managedEntryPointInfo = entryPointInfo.Value;
        _gameExecutionContext = gameExecutionContext;
        _debuggerSettings = debuggerSettings.Value;

        _hookGetProcAddressDelegate = HookGetProcAddress;
        _monoInitDetourFn = MonoJitInitDetour;
        _jitParseOptionsDetourFn = MonoJitParseOptionsDetour;
        _debugInitDetourFn = MonoDebugInitDetour;
        _symbolRedirects = new Dictionary<string, IntPtr>
        {
            { "mono_jit_init_version", Marshal.GetFunctionPointerForDelegate(_monoInitDetourFn)},
            { "mono_jit_parse_options", Marshal.GetFunctionPointerForDelegate(_jitParseOptionsDetourFn)},
            { "mono_debug_init", Marshal.GetFunctionPointerForDelegate(_debugInitDetourFn)},
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bootstrapping Mono...");
        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "GetProcAddress", Marshal.GetFunctionPointerForDelegate(_hookGetProcAddressDelegate));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Unity obtains Mono's symbols by calling GetProcAddress on them as opposed to calling them via their imports.
    // This means we can't PltHook them directly, but we can PltHook GetProcAddress and redirect the ones we're
    // interested in which is what this hook does. It also gives us the Mono's handle as an added bonus since we need it
    private nint HookGetProcAddress(HMODULE handle, PCSTR symbol)
    {
        var originalSymbolAddress = PInvoke.GetProcAddress(handle, symbol);
        if (!_symbolRedirects.TryGetValue(symbol.ToString(), out var detourAddress))
            return originalSymbolAddress;

        if (!_runtimeInitialised)
            RetrieveMonoExports(handle);
        _runtimeInitialised = true;

        _logger.LogInformation("Redirecting {Symbol}", symbol);
        return detourAddress;
    }

    private void RetrieveMonoExports(nint handle)
    {
        _logger.LogInformation("Loading Mono exports");
        Mono = new Mono
        {
            RuntimeInvoke = NativeLibrary.GetExportDelegate<Mono.RuntimeInvokeFn>(handle, "mono_runtime_invoke"),
            JitInitVersion = NativeLibrary.GetExportDelegate<Mono.JitInitVersionFn>(handle, "mono_jit_init_version"),
            JitParseOptions = NativeLibrary.GetExportDelegate<Mono.JitParseOptionsFn>(handle, "mono_jit_parse_options"),
            ThreadCurrent = NativeLibrary.GetExportDelegate<Mono.ThreadCurrentFn>(handle, "mono_thread_current"),
            DebugEnabled = NativeLibrary.GetExportDelegate<Mono.DebugEnabledFn>(handle, "mono_debug_enabled"),
            DebugInit = NativeLibrary.GetExportDelegate<Mono.DebugInitFn>(handle, "mono_debug_init"),
            ThreadSetMain = NativeLibrary.GetExportDelegate<Mono.ThreadSetMainFn>(handle, "mono_thread_set_main"),
            SetAssembliesPath = NativeLibrary.GetExportDelegate<Mono.SetAssembliesPathFn>(handle, "mono_set_assemblies_path"),
            AssemblyGetrootdir = NativeLibrary.GetExportDelegate<Mono.AssemblyGetrootdirFn>(handle, "mono_assembly_getrootdir"),
            DomainAssemblyOpen = NativeLibrary.GetExportDelegate<Mono.DomainAssemblyOpenFn>(handle, "mono_domain_assembly_open"),
            AssemblyGetImage = NativeLibrary.GetExportDelegate<Mono.AssemblyGetImageFn>(handle, "mono_assembly_get_image"),
            ClassFromName = NativeLibrary.GetExportDelegate<Mono.ClassFromNameFn>(handle, "mono_class_from_name"),
            ClassGetMethodFromName = NativeLibrary.GetExportDelegate<Mono.ClassGetMethodFromNameFn>(handle, "mono_class_get_method_from_name"),
            DomainSetConfig = NativeLibrary.GetExportDelegate<Mono.DomainSetConfigFn>(handle, "mono_domain_set_config"),
            ConfigParse = NativeLibrary.GetExportDelegate<Mono.ConfigParseFn>(handle, "mono_config_parse")
        };
    }

    // This hooks the main Mono initialization function so it contains most of the machinery needed.
    // Most of the initialization process replicates what Unity does so we're effectively seizing control of the runtime
    private nint MonoJitInitDetour(nint domainName, nint runtimeVersion)
    {
        if (_jitInitDone)
            return Mono.JitInitVersion(domainName, runtimeVersion);

        _logger.LogInformation("In init detour");
        string domainNameStr = Marshal.PtrToStringAnsi(domainName)!;
        string runtimeVersionStr = Marshal.PtrToStringAnsi(runtimeVersion)!;
        _logger.LogInformation("Domain: {DomainNameStr}, Runtime version: {RuntimeVersionStr}", domainNameStr, runtimeVersionStr);

        SetMonoAssembliesPath();

        MonoJitParseOptionsDetour(0, []);
        InitialiseMonoDebuggerIfNeeded();

        _logger.LogInformation("Original init jit version");
        Domain = Mono.JitInitVersion(domainName, runtimeVersion);

        SetMonoMainThreadToCurrentThread();
        SetupMonoConfigs();

        TransitionToMonoManagedSide();

        _jitInitDone = true;
        return Domain;
    }

    private void SetupMonoConfigs()
    {
        string configFile = $"{Environment.ProcessPath}.config";
        _logger.LogInformation($"Setting Mono Config paths: base_dir: {_gameExecutionContext.GameDir}, config_file_name: {configFile}");
        Mono.DomainSetConfig(Domain, _gameExecutionContext.GameDir, configFile);

        _logger.LogInformation("Parsing default Mono config");
        Mono.ConfigParse(null);
    }

    private void SetMonoMainThreadToCurrentThread()
    {
        _logger.LogInformation("Setting Mono Main Thread");
        Mono.ThreadSetMain(Mono.ThreadCurrent());
    }

    private void InitialiseMonoDebuggerIfNeeded()
    {
        bool debuggerAlreadyEnabled = _debugInitCalled || Mono.DebugEnabled();
        if (debuggerAlreadyEnabled || !_debuggerSettings.Enable!.Value)
            return;

        _logger.LogInformation("Initialising Mono debugger");
        Mono.DebugInit(Mono.MonoDebugFormat.MonoDebugFormatMono);
    }

    private void SetMonoAssembliesPath()
    {
        StringBuilder newAssembliesPathSb = new();
        _additionalMonoAssembliesPath = Path.Combine(_gameExecutionContext.GameDir, "UnityJitMonoBcl");
        newAssembliesPathSb.Append(_additionalMonoAssembliesPath);
        newAssembliesPathSb.Append(';');
        newAssembliesPathSb.Append(Mono.AssemblyGetrootdir());

        string newAssembliesPath = newAssembliesPathSb.ToString();
        _logger.LogInformation("Setting Mono assemblies path to: {NewAssembliesPath}", newAssembliesPath);
        Mono.SetAssembliesPath(newAssembliesPath);
    }

    // This hooks appends debugger arguments to enable Mono's SDB server
    private void MonoJitParseOptionsDetour(nint argc, string[] argv)
    {
        if (_jitInitDone)
        {
            Mono.JitParseOptions(argc, argv);
            return;
        }
        _logger.LogInformation("jit parse options");

        string newArgs;
        string? dnSpyEnv = Environment.GetEnvironmentVariable("DNSPY_UNITY_DBG2");
        if (dnSpyEnv is not null)
        {
            newArgs = dnSpyEnv;
        }
        else if (_debuggerSettings.Enable!.Value)
        {
            StringBuilder newArgsSb = new(MonoDebugArgsStart);
            newArgsSb.Append(_debuggerSettings.IpAddress);
            newArgsSb.Append(':');
            newArgsSb.Append(_debuggerSettings.Port);
            if (!_debuggerSettings.SuspendOnBoot!.Value)
                newArgsSb.Append(MonoDebugNoSuspendArg);
            newArgs = newArgsSb.ToString();
        }
        else
        {
            Mono.JitParseOptions(argc, argv);
            return;
        }

        string[] newArgv = new string[argc + 1];
        Array.Copy(argv, 0, newArgv, 0, argc);
        argc++;
        newArgv[argc - 1] = newArgs;

        _logger.LogInformation("Adding jit options: {NewJitOptions}", string.Join(' ', newArgs));

        Mono.JitParseOptions(argc, newArgv);
    }

    // It's possible that Mono already wants to initialise its SDB server. We don't have a problem with this, but we do
    // need to make sure that we don't initialise it twice so this hook is still needed to detect this
    private void MonoDebugInitDetour(Mono.MonoDebugFormat format)
    {
        _logger.LogInformation("Debug init");
        _debugInitCalled = true;
        Mono.DebugInit(format);
    }

    private unsafe void TransitionToMonoManagedSide()
    {
        _logger.LogInformation("Loading entrypoint assembly");
        var assembly = Mono.DomainAssemblyOpen(Domain, _managedEntryPointInfo.AssemblyPath);
        if (assembly == 0)
        {
            _logger.LogCritical("Failed to load the entrypoint assembly into the Mono domain");
            return;
        }

        var image = Mono.AssemblyGetImage(assembly);
        var interopClass = Mono.ClassFromName(image, _managedEntryPointInfo.Namespace, _managedEntryPointInfo.ClassName);
        var initMethod = Mono.ClassGetMethodFromName(interopClass, _managedEntryPointInfo.MethodName, 0);

        nint ex = 0;
        var initArgs = stackalloc nint*[] { };
        _logger.LogInformation("Invoking entrypoint method");
        Mono.RuntimeInvoke(initMethod, 0, (void**)initArgs, ref ex);
    }
}
