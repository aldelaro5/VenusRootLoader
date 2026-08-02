using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Unity;
using VenusRootLoader.Bootstrap.Unity.GlobalManagers;
using Windows.Win32.Foundation;
using Environment = System.Environment;

namespace VenusRootLoader.Bootstrap.Mono;

/// <summary>
/// <para>
/// This service uses an instance of <see cref="MonoFunctions"/> to initialise the Mono runtime using various hooks,
/// and transitions to the managed side using the newly initialised runtime.
/// </para>
/// <para>
/// The hooks are done by hooking on GetProcAddress because this is how Unity discovers the symbols of Mono.
/// We can simply detour the symbols we are interested in by returning our own version and decide whether
/// to call the original or not
/// </para>
/// </summary>
internal sealed class MonoInitializer
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
    private readonly IMonoFunctions _monoFunctions;
    private string _additionalMonoAssembliesPath = string.Empty;

    private const string MonoDebugArgsStart = "--debugger-agent=transport=dt_socket,server=y,address=";
    private const string MonoDebugNoSuspendArg = ",suspend=n";

    private static IMonoFunctions.JitInitVersionFn _monoInitDetourFn = null!;
    private static IMonoFunctions.JitParseOptionsFn _jitParseOptionsDetourFn = null!;
    private static IMonoFunctions.DebugInitFn _debugInitDetourFn = null!;
    private static IMonoFunctions.ImageOpenFromDataWithNameFn _imageOpenFromDataWithNameDetourFn = null!;

    private readonly Dictionary<string, nint> _symbolRedirects;

    private readonly IFileSystem _fileSystem;
    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ILogger _logger;
    private readonly IGameExecutionContext _gameExecutionContext;
    private readonly MonoDebuggerSettings _debuggerSettings;
    private readonly IPlayerConnectionDiscovery _playerConnectionDiscovery;
    private readonly ISdbWinePathTranslator _sdbWinePathTranslator;
    private readonly IMonoInitLifeCycleEvents _monoInitLifeCycleEvents;
    private readonly IAssembliesListAppender _assembliesListAppender;
    private readonly nint _logFunctionPtr;
    private readonly nint _gameExecutionContextPtr;
    private readonly nint _basePathPtr;

    public unsafe MonoInitializer(
        ILogger<MonoInitializer> logger,
        IPltHooksManager pltHooksManager,
        IGameExecutionContext gameExecutionContext,
        IBootstrapEnvironment bootstrapEnvironment,
        IOptions<MonoDebuggerSettings> debuggerSettings,
        IPlayerConnectionDiscovery playerConnectionDiscovery,
        ISdbWinePathTranslator sdbWinePathTranslator,
        IMonoInitLifeCycleEvents monoInitLifeCycleEvents,
        IWin32 win32,
        IFileSystem fileSystem,
        IMonoFunctions monoFunctions,
        IAssembliesListAppender assembliesListAppender)
    {
        _logger = logger;
        _pltHooksManager = pltHooksManager;
        _sdbWinePathTranslator = sdbWinePathTranslator;
        _monoInitLifeCycleEvents = monoInitLifeCycleEvents;
        _win32 = win32;
        _fileSystem = fileSystem;
        _monoFunctions = monoFunctions;
        _assembliesListAppender = assembliesListAppender;

        _gameExecutionContext = gameExecutionContext;
        _playerConnectionDiscovery = playerConnectionDiscovery;
        _debuggerSettings = debuggerSettings.Value;

        _logFunctionPtr = Marshal.GetFunctionPointerForDelegate(ManagedLogsRelay.RelayLogFunction);

        _gameExecutionContextPtr = _gameExecutionContext.GetPointer();
        _basePathPtr = Marshal.StringToHGlobalUni(bootstrapEnvironment.BasePath);

        _hookGetProcAddressDelegate = HookGetProcAddress;
        _monoInitDetourFn = MonoJitInitDetour;
        _jitParseOptionsDetourFn = MonoJitParseOptionsDetour;
        _debugInitDetourFn = MonoDebugInitDetour;
        _imageOpenFromDataWithNameDetourFn = MonoImageOpenFromDataWithNameDetour;
        _symbolRedirects = new Dictionary<string, IntPtr>
        {
            { "mono_jit_init_version", Marshal.GetFunctionPointerForDelegate(_monoInitDetourFn) },
            { "mono_jit_parse_options", Marshal.GetFunctionPointerForDelegate(_jitParseOptionsDetourFn) },
            { "mono_debug_init", Marshal.GetFunctionPointerForDelegate(_debugInitDetourFn) },
            {
                "mono_image_open_from_data_with_name",
                Marshal.GetFunctionPointerForDelegate(_imageOpenFromDataWithNameDetourFn)
            }
        };
    }

    public void HookMonoInitialization()
    {
        _pltHooksManager.InstallHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "GetProcAddress",
            _hookGetProcAddressDelegate);
    }

    private unsafe nint MonoImageOpenFromDataWithNameDetour(
        byte* data,
        uint dataLen,
        bool needCopy,
        ref IMonoFunctions.MonoImageOpenStatus status,
        bool refOnly,
        string name)
    {
        return _monoFunctions.ImageOpenFromDataWithName(
            data,
            dataLen,
            needCopy,
            ref status,
            refOnly,
            _assembliesListAppender.OnMonoImageOpenFromDataWithName(name));
    }

    private unsafe nint HookGetProcAddress(HMODULE handle, PCSTR symbol)
    {
        FARPROC originalSymbolAddress = _win32.GetProcAddress(handle, symbol);
        if (!_symbolRedirects.TryGetValue(symbol.ToString(), out IntPtr detourAddress))
            return originalSymbolAddress;

        if (!_runtimeInitialised)
        {
            RetrieveMonoExports(handle);
            if (_gameExecutionContext.IsWine && _debuggerSettings.Enable!.Value)
            {
                fixed (char* monoFileNamePtr = new char[2048])
                {
                    _win32.GetModuleFileName(handle, new PWSTR(monoFileNamePtr), 2048);
                    string monoFileName = Marshal.PtrToStringUni((nint)monoFileNamePtr)!;
                    _sdbWinePathTranslator.Setup(monoFileName);
                }
            }
        }

        _runtimeInitialised = true;

        _logger.LogDebug("Redirecting {Symbol}", symbol);
        return detourAddress;
    }

    private void RetrieveMonoExports(HMODULE handle)
    {
        _logger.LogDebug("Loading Mono exports");
        _monoFunctions.Initialize(handle);
    }

    // This hooks the main Mono initialization function so it contains most of the machinery needed.
    // Most of the initialization process replicates what Unity does so we're effectively seizing control of the runtime
    private nint MonoJitInitDetour(nint domainName, nint runtimeVersion)
    {
        if (_jitInitDone)
            return _monoFunctions.JitInitVersion(domainName, runtimeVersion);

        _logger.LogInformation("In init detour");
        string domainNameStr = Marshal.PtrToStringAnsi(domainName)!;
        string runtimeVersionStr = Marshal.PtrToStringAnsi(runtimeVersion)!;
        _logger.LogInformation(
            "Domain: {DomainNameStr}, Runtime version: {RuntimeVersionStr}",
            domainNameStr,
            runtimeVersionStr);

        SetMonoAssembliesPath();

        MonoJitParseOptionsDetour(0, []);
        InitialiseMonoDebuggerIfNeeded();

        // Debugger means we need the player discovery service so IDEs can find this player
        if (_debuggerSettings.Enable!.Value)
        {
            // If mono_debug_init was called, it means that this is a dev build of UnityPlayer.dll which changes how
            // the player discovery is done (check the player discovery service documentation to learn more)
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

        _monoInitLifeCycleEvents.Publish(this);

        // At this point, we are nearing the end of the bootstrap's lifecycle so we know we won't need to hook on further
        // Mono symbols
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "GetProcAddress");

        _logger.LogInformation("Original init jit version");
        if (_debuggerSettings.Enable.Value && _debuggerSettings.SuspendOnBoot!.Value)
            _logger.LogInformation("Waiting until a debugger is attached...");
        Domain = _monoFunctions.JitInitVersion(domainName, runtimeVersion);
        if (_debuggerSettings.Enable.Value && _debuggerSettings.SuspendOnBoot!.Value)
            _logger.LogInformation("Debugger attached! Resuming boot");

        SetMonoMainThreadToCurrentThread();
        SetupMonoConfigs();

        TransitionToMonoManagedSide(
            new()
            {
                AssemblyPath =
                    _fileSystem.Path.Combine(
                        _gameExecutionContext.GameDir,
                        "VenusRootLoader",
                        "VenusRootLoader.Preloader.dll"),
                Namespace = "VenusRootLoader.Preloader",
                ClassName = "Entry",
                MethodName = "Main"
            });

        _jitInitDone = true;
        return Domain;
    }

    private void SetupMonoConfigs()
    {
        string configFile = $"{Environment.ProcessPath}.config";
        _logger.LogDebug(
            $"Setting Mono Config paths: base_dir: {_gameExecutionContext.GameDir}, config_file_name: {configFile}");
        _monoFunctions.DomainSetConfig(Domain, _gameExecutionContext.GameDir, configFile);

        _logger.LogDebug("Parsing default Mono config");
        _monoFunctions.ConfigParse(null);
    }

    private void SetMonoMainThreadToCurrentThread()
    {
        _logger.LogDebug("Setting Mono Main Thread");
        _monoFunctions.ThreadSetMain(_monoFunctions.ThreadCurrent());
    }

    private void InitialiseMonoDebuggerIfNeeded()
    {
        // If this is a dev build, it's possible to get a double call which is bad so we want to prevent this
        bool debuggerAlreadyEnabled = _debugInitCalled || _monoFunctions.DebugEnabled();
        if (debuggerAlreadyEnabled || !_debuggerSettings.Enable!.Value)
            return;

        _logger.LogInformation("Initialising Mono debugger");
        _monoFunctions.DebugInit(IMonoFunctions.MonoDebugFormat.MonoDebugFormatMono);
    }

    // This is what implements the BCL unstripping automatically without needing for any configuration.
    // The BCL assemblies were selected from a Unity 2018.4.12f1 install, see the README in the UnityJitMonoBCL
    // folder to learn more
    private void SetMonoAssembliesPath()
    {
        StringBuilder newAssembliesPathSb = new();
        _additionalMonoAssembliesPath = _fileSystem.Path.Combine(_gameExecutionContext.GameDir, "UnityJitMonoBcl");
        newAssembliesPathSb.Append(_additionalMonoAssembliesPath);
        newAssembliesPathSb.Append(';');
        newAssembliesPathSb.Append(_monoFunctions.AssemblyGetrootdir());

        string newAssembliesPath = newAssembliesPathSb.ToString();
        _logger.LogInformation("Setting Mono assemblies path to: {NewAssembliesPath}", newAssembliesPath);
        _monoFunctions.SetAssembliesPath(newAssembliesPath);
    }

    // This hooks appends debugger arguments to enable Mono's SDB server
    private void MonoJitParseOptionsDetour(nint argc, string[] argv)
    {
        if (_jitInitDone)
        {
            _monoFunctions.JitParseOptions(argc, argv);
            return;
        }

        _logger.LogInformation("jit parse options");

        string newArgs;
        string? dnSpyEnv = Environment.GetEnvironmentVariable("DNSPY_UNITY_DBG2");
        if (dnSpyEnv is not null)
        {
            newArgs = dnSpyEnv;
            _logger.LogInformation(
                "Overriding the Mono debugging configuration by the DNSPY_UNITY_DBG2 environment variable");
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
            _monoFunctions.JitParseOptions(argc, argv);
            return;
        }

        string[] newArgv = new string[argc + 1];
        Array.Copy(argv, 0, newArgv, 0, argc);
        argc++;
        newArgv[argc - 1] = newArgs;

        _logger.LogInformation("Adding jit options: {NewJitOptions}", string.Join(' ', newArgs));

        _monoFunctions.JitParseOptions(argc, newArgv);
    }

    // It's possible that Mono already wants to initialise its SDB server. We don't have a problem with this, but we do
    // need to make sure that we don't initialise it twice so this hook is still needed to detect this
    private void MonoDebugInitDetour(IMonoFunctions.MonoDebugFormat format)
    {
        _logger.LogInformation("Debug init");
        _debugInitCalled = true;
        _monoFunctions.DebugInit(format);
    }

    private unsafe void TransitionToMonoManagedSide(ManagedEntryPointInfo entryPointInfo)
    {
        _logger.LogInformation("Loading entrypoint assembly");
        IntPtr assembly = _monoFunctions.DomainAssemblyOpen(Domain, entryPointInfo.AssemblyPath);
        if (assembly == 0)
        {
            _logger.LogCritical("Failed to load the entrypoint assembly into the Mono domain");
            return;
        }

        IntPtr image = _monoFunctions.AssemblyGetImage(assembly);
        IntPtr interopClass = _monoFunctions.ClassFromName(image, entryPointInfo.Namespace, entryPointInfo.ClassName);
        IntPtr initMethod = _monoFunctions.ClassGetMethodFromName(interopClass, entryPointInfo.MethodName, 3);

        nint ex = 0;

        fixed (void* logFunctionPtr = &_logFunctionPtr,
               gameExecutionContextPtr = &_gameExecutionContextPtr,
               basePathPtr = &_basePathPtr)
        {
            void** initArgs = stackalloc void*[]
            {
                logFunctionPtr,
                gameExecutionContextPtr,
                basePathPtr
            };
            _logger.LogInformation("Invoking entrypoint method");
            _monoFunctions.RuntimeInvoke(initMethod, 0, initArgs, ref ex);
        }
    }
}