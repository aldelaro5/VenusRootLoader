using System.IO.Abstractions;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32.Foundation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Extensions;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Unity;

namespace VenusRootLoader.Bootstrap.Mono;

/// <summary>
/// This class initialises an instance of <see cref="MonoFunctions"/>, initialises the Mono runtime using various hooks,
/// and transitions to the managed side using the newly initialised runtime
/// </summary>
internal class MonoInitializer : IHostedService
{
    private struct ManagedEntryPointInfo
    {
        internal required string AssemblyPath { get; init; }
        internal required string Namespace { get; init; }
        internal required string ClassName { get; init; }
        internal required string MethodName { get; init; }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate nint GetProcAddressFn(HMODULE handle, PCSTR symbol);
    private static GetProcAddressFn _hookGetProcAddressDelegate = null!;

    private bool _runtimeInitialised;
    private bool _debugInitCalled;
    private bool _jitInitDone;

    private nint Domain { get; set; }
    private MonoFunctions MonoFunctions { get; set; } = null!;
    private string _additionalMonoAssembliesPath = string.Empty;

    private const string MonoDebugArgsStart = "--debugger-agent=transport=dt_socket,server=y,address=";
    private const string MonoDebugNoSuspendArg = ",suspend=n";

    private static MonoFunctions.JitInitVersionFn _monoInitDetourFn = null!;
    private static MonoFunctions.JitParseOptionsFn _jitParseOptionsDetourFn = null!;
    private static MonoFunctions.DebugInitFn _debugInitDetourFn = null!;

    private readonly Dictionary<string, nint> _symbolRedirects;

    private readonly IFileSystem _fileSystem;
    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ILogger _logger;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly MonoDebuggerSettings _debuggerSettings;
    private readonly PlayerConnectionDiscovery _playerConnectionDiscovery;
    private readonly SdbWinePathTranslator _sdbWinePathTranslator;
    private readonly GameLifecycleEvents _gameLifecycleEvents;
    private readonly IHostEnvironment _hostEnvironment;

    public MonoInitializer(
        ILogger<MonoInitializer> logger,
        IPltHooksManager pltHooksManager,
        GameExecutionContext gameExecutionContext,
        IOptions<MonoDebuggerSettings> debuggerSettings,
        PlayerConnectionDiscovery playerConnectionDiscovery,
        SdbWinePathTranslator sdbWinePathTranslator,
        GameLifecycleEvents gameLifecycleEvents,
        IHostEnvironment hostEnvironment,
        IWin32 win32,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _pltHooksManager = pltHooksManager;
        _sdbWinePathTranslator = sdbWinePathTranslator;
        _gameLifecycleEvents = gameLifecycleEvents;
        _hostEnvironment = hostEnvironment;
        _win32 = win32;
        _fileSystem = fileSystem;

        _gameExecutionContext = gameExecutionContext;
        _playerConnectionDiscovery = playerConnectionDiscovery;
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
        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "GetProcAddress", Marshal.GetFunctionPointerForDelegate(_hookGetProcAddressDelegate));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Unity obtains Mono's symbols by calling GetProcAddress on them as opposed to calling them via their imports.
    // This means we can't PltHook them directly, but we can PltHook GetProcAddress and redirect the ones we're
    // interested in which is what this hook does. It also gives us the Mono's handle as an added bonus since we need it
    private unsafe nint HookGetProcAddress(HMODULE handle, PCSTR symbol)
    {
        var originalSymbolAddress = _win32.GetProcAddress(handle, symbol);
        if (!_symbolRedirects.TryGetValue(symbol.ToString(), out var detourAddress))
            return originalSymbolAddress;

        if (!_runtimeInitialised)
        {
            RetrieveMonoExports(handle);
            if (_gameExecutionContext.IsWine && _debuggerSettings.Enable!.Value)
            {
                fixed (char* monoFileNamePtr = new char[2048])
                {
                    _win32.GetModuleFileName(handle, new PWSTR(monoFileNamePtr), 2048);
                    var monoFileName = Marshal.PtrToStringAuto((nint)monoFileNamePtr)!;
                    _sdbWinePathTranslator.Setup(monoFileName);
                }
            }
        }

        _runtimeInitialised = true;

        _logger.LogInformation("Redirecting {Symbol}", symbol);
        return detourAddress;
    }

    private void RetrieveMonoExports(nint handle)
    {
        _logger.LogInformation("Loading Mono exports");
        MonoFunctions = new MonoFunctions
        {
            RuntimeInvoke = NativeLibrary.GetExportDelegate<MonoFunctions.RuntimeInvokeFn>(handle, "mono_runtime_invoke"),
            JitInitVersion = NativeLibrary.GetExportDelegate<MonoFunctions.JitInitVersionFn>(handle, "mono_jit_init_version"),
            JitParseOptions = NativeLibrary.GetExportDelegate<MonoFunctions.JitParseOptionsFn>(handle, "mono_jit_parse_options"),
            ThreadCurrent = NativeLibrary.GetExportDelegate<MonoFunctions.ThreadCurrentFn>(handle, "mono_thread_current"),
            DebugEnabled = NativeLibrary.GetExportDelegate<MonoFunctions.DebugEnabledFn>(handle, "mono_debug_enabled"),
            DebugInit = NativeLibrary.GetExportDelegate<MonoFunctions.DebugInitFn>(handle, "mono_debug_init"),
            ThreadSetMain = NativeLibrary.GetExportDelegate<MonoFunctions.ThreadSetMainFn>(handle, "mono_thread_set_main"),
            SetAssembliesPath = NativeLibrary.GetExportDelegate<MonoFunctions.SetAssembliesPathFn>(handle, "mono_set_assemblies_path"),
            AssemblyGetrootdir = NativeLibrary.GetExportDelegate<MonoFunctions.AssemblyGetrootdirFn>(handle, "mono_assembly_getrootdir"),
            DomainAssemblyOpen = NativeLibrary.GetExportDelegate<MonoFunctions.DomainAssemblyOpenFn>(handle, "mono_domain_assembly_open"),
            AssemblyGetImage = NativeLibrary.GetExportDelegate<MonoFunctions.AssemblyGetImageFn>(handle, "mono_assembly_get_image"),
            ClassFromName = NativeLibrary.GetExportDelegate<MonoFunctions.ClassFromNameFn>(handle, "mono_class_from_name"),
            ClassGetMethodFromName = NativeLibrary.GetExportDelegate<MonoFunctions.ClassGetMethodFromNameFn>(handle, "mono_class_get_method_from_name"),
            DomainSetConfig = NativeLibrary.GetExportDelegate<MonoFunctions.DomainSetConfigFn>(handle, "mono_domain_set_config"),
            ConfigParse = NativeLibrary.GetExportDelegate<MonoFunctions.ConfigParseFn>(handle, "mono_config_parse")
        };
    }

    // This hooks the main Mono initialization function so it contains most of the machinery needed.
    // Most of the initialization process replicates what Unity does so we're effectively seizing control of the runtime
    private nint MonoJitInitDetour(nint domainName, nint runtimeVersion)
    {
        if (_jitInitDone)
            return MonoFunctions.JitInitVersion(domainName, runtimeVersion);

        _logger.LogInformation("In init detour");
        string domainNameStr = Marshal.PtrToStringAnsi(domainName)!;
        string runtimeVersionStr = Marshal.PtrToStringAnsi(runtimeVersion)!;
        _logger.LogInformation("Domain: {DomainNameStr}, Runtime version: {RuntimeVersionStr}", domainNameStr, runtimeVersionStr);

        SetMonoAssembliesPath();

        MonoJitParseOptionsDetour(0, []);
        InitialiseMonoDebuggerIfNeeded();

        _logger.LogInformation("Original init jit version");
        if (_debuggerSettings.Enable!.Value)
        {
            if (_debugInitCalled)
            {
                _playerConnectionDiscovery.StartDiscoveryWithSendToHook(
                    _debuggerSettings.IpAddress,
                    (ushort)_debuggerSettings.Port!.Value);
            }
            else
            {
                _playerConnectionDiscovery.StartDiscoveryWithOwnSocket(
                    _debuggerSettings.IpAddress,
                    (ushort)_debuggerSettings.Port!.Value);
            }
        }

        _gameLifecycleEvents.Publish(this, new() { LifeCycle = GameLifecycle.MonoInitialising });
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "GetProcAddress");
        Domain = MonoFunctions.JitInitVersion(domainName, runtimeVersion);

        SetMonoMainThreadToCurrentThread();
        SetupMonoConfigs();

        TransitionToMonoManagedSide(new()
        {
            AssemblyPath = _fileSystem.Path.Combine(_hostEnvironment.ContentRootPath, "VenusRootLoader", "VenusRootLoader.dll"),
            Namespace = "VenusRootLoader",
            ClassName = "MonoInitEntry",
            MethodName = "Main"
        });

        _jitInitDone = true;
        return Domain;
    }

    private void SetupMonoConfigs()
    {
        string configFile = $"{Environment.ProcessPath}.config";
        _logger.LogInformation($"Setting Mono Config paths: base_dir: {_gameExecutionContext.GameDir}, config_file_name: {configFile}");
        MonoFunctions.DomainSetConfig(Domain, _gameExecutionContext.GameDir, configFile);

        _logger.LogInformation("Parsing default Mono config");
        MonoFunctions.ConfigParse(null);
    }

    private void SetMonoMainThreadToCurrentThread()
    {
        _logger.LogInformation("Setting Mono Main Thread");
        MonoFunctions.ThreadSetMain(MonoFunctions.ThreadCurrent());
    }

    private void InitialiseMonoDebuggerIfNeeded()
    {
        bool debuggerAlreadyEnabled = _debugInitCalled || MonoFunctions.DebugEnabled();
        if (debuggerAlreadyEnabled || !_debuggerSettings.Enable!.Value)
            return;

        _logger.LogInformation("Initialising Mono debugger");
        MonoFunctions.DebugInit(MonoFunctions.MonoDebugFormat.MonoDebugFormatMono);
    }

    private void SetMonoAssembliesPath()
    {
        StringBuilder newAssembliesPathSb = new();
        _additionalMonoAssembliesPath = _fileSystem.Path.Combine(_hostEnvironment.ContentRootPath, "UnityJitMonoBcl");
        newAssembliesPathSb.Append(_additionalMonoAssembliesPath);
        newAssembliesPathSb.Append(';');
        newAssembliesPathSb.Append(MonoFunctions.AssemblyGetrootdir());

        string newAssembliesPath = newAssembliesPathSb.ToString();
        _logger.LogInformation("Setting Mono assemblies path to: {NewAssembliesPath}", newAssembliesPath);
        MonoFunctions.SetAssembliesPath(newAssembliesPath);
    }

    // This hooks appends debugger arguments to enable Mono's SDB server
    private void MonoJitParseOptionsDetour(nint argc, string[] argv)
    {
        if (_jitInitDone)
        {
            MonoFunctions.JitParseOptions(argc, argv);
            return;
        }
        _logger.LogInformation("jit parse options");

        string newArgs;
        string? dnSpyEnv = Environment.GetEnvironmentVariable("DNSPY_UNITY_DBG2");
        if (dnSpyEnv is not null)
        {
            newArgs = dnSpyEnv;
            _logger.LogInformation("Overriding the Mono debugging configuration by the DNSPY_UNITY_DBG2 environment variable");
        }
        else if (_debuggerSettings.Enable!.Value)
        {
            StringBuilder newArgsSb = new(MonoDebugArgsStart);
            newArgsSb.Append(IPAddress.Parse(_debuggerSettings.IpAddress));
            newArgsSb.Append(':');
            newArgsSb.Append(_debuggerSettings.Port);
            if (!_debuggerSettings.SuspendOnBoot!.Value)
                newArgsSb.Append(MonoDebugNoSuspendArg);
            newArgs = newArgsSb.ToString();
        }
        else
        {
            MonoFunctions.JitParseOptions(argc, argv);
            return;
        }

        string[] newArgv = new string[argc + 1];
        Array.Copy(argv, 0, newArgv, 0, argc);
        argc++;
        newArgv[argc - 1] = newArgs;

        _logger.LogInformation("Adding jit options: {NewJitOptions}", string.Join(' ', newArgs));

        MonoFunctions.JitParseOptions(argc, newArgv);
    }

    // It's possible that Mono already wants to initialise its SDB server. We don't have a problem with this, but we do
    // need to make sure that we don't initialise it twice so this hook is still needed to detect this
    private void MonoDebugInitDetour(MonoFunctions.MonoDebugFormat format)
    {
        _logger.LogInformation("Debug init");
        _debugInitCalled = true;
        MonoFunctions.DebugInit(format);
    }

    private unsafe void TransitionToMonoManagedSide(ManagedEntryPointInfo entryPointInfo)
    {
        _logger.LogInformation("Loading entrypoint assembly");
        var assembly = MonoFunctions.DomainAssemblyOpen(Domain, entryPointInfo.AssemblyPath);
        if (assembly == 0)
        {
            _logger.LogCritical("Failed to load the entrypoint assembly into the Mono domain");
            return;
        }

        var image = MonoFunctions.AssemblyGetImage(assembly);
        var interopClass = MonoFunctions.ClassFromName(image, entryPointInfo.Namespace, entryPointInfo.ClassName);
        var initMethod = MonoFunctions.ClassGetMethodFromName(interopClass, entryPointInfo.MethodName, 0);

        nint ex = 0;
        var initArgs = stackalloc nint*[] { };
        _logger.LogInformation("Invoking entrypoint method");
        MonoFunctions.RuntimeInvoke(initMethod, 0, (void**)initArgs, ref ex);
    }
}
