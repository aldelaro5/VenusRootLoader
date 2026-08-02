using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using System.Text;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;
using VenusRootLoader.Bootstrap.Unity.GlobalManagers;
using Windows.Win32.Foundation;

namespace VenusRootLoader.Bootstrap.Tests.Mono;

[Collection(nameof(MonoInitializerTests))]
public sealed class MonoInitializerTests
{
    private readonly FakeLogger<MonoInitializer> _logger = new();

    private readonly IOptions<MonoDebuggerSettings>
        _debuggerSettings = Substitute.For<IOptions<MonoDebuggerSettings>>();

    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly IBootstrapEnvironment _bootstrapEnvironment = Substitute.For<IBootstrapEnvironment>();

    private readonly IPlayerConnectionDiscovery _playerConnectionDiscovery =
        Substitute.For<IPlayerConnectionDiscovery>();

    private readonly ISdbWinePathTranslator _sdbWinePathTranslator = Substitute.For<ISdbWinePathTranslator>();
    private readonly IMonoFunctions _monoFunctions = Substitute.For<IMonoFunctions>();
    private readonly IMonoInitLifeCycleEvents _monoInitLifeCycleEvents = Substitute.For<IMonoInitLifeCycleEvents>();
    private readonly IAssembliesListAppender _assembliesListAppender = Substitute.For<IAssembliesListAppender>();
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly MockFileSystem _fileSystem = new();
    private IGameExecutionContext _gameExecutionContext = Substitute.For<IGameExecutionContext>();

    private readonly MonoDebuggerSettings _debuggerSettingsValue = new()
    {
        Enable = false,
        IpAddress = "0.0.0.0",
        Port = 55555,
        SuspendOnBoot = false
    };

    private readonly MonoInitializer _sut;

    public MonoInitializerTests()
    {
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", null);
        _debuggerSettings.Value.Returns(_debuggerSettingsValue);
        _sut = new(
            _logger,
            _pltHooksManager,
            _gameExecutionContext,
            _bootstrapEnvironment,
            _debuggerSettings,
            _playerConnectionDiscovery,
            _sdbWinePathTranslator,
            _monoInitLifeCycleEvents,
            _win32,
            _fileSystem,
            _monoFunctions,
            _assembliesListAppender);
    }

    [Fact]
    public void HookMonoInitialization_InstallsGetProcAddressHook_WhenCalled()
    {
        _sut.HookMonoInitialization();
        _pltHooksManager.Hooks.Should()
            .ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.GetProcAddress)));
    }

    [Fact]
    public unsafe void GetProcAddressHook_ReturnOriginalResult_WhenSymbolIsNotOfInterest()
    {
        _sut.HookMonoInitialization();

        string symbol = "SomeFunction";
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);

        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;

        result.Should().Be(symbolAddress);
        _win32.Received(1).GetProcAddress(moduleHandle, symbolPtr);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Theory]
    [InlineData("mono_jit_init_version")]
    [InlineData("mono_jit_parse_options")]
    [InlineData("mono_debug_init")]
    public unsafe void GetProcAddressHook_CallsOriginalAndModifyReturn_WhenSymbolIsOfInterest(string symbol)
    {
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);

        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;

        result.Should().NotBe(symbolAddress);
        _win32.Received(1).GetProcAddress(moduleHandle, symbolPtr);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Theory]
    [InlineData("mono_jit_init_version")]
    [InlineData("mono_jit_parse_options")]
    [InlineData("mono_debug_init")]
    public unsafe void GetProcAddressHook_SetupSdbTranslator_WhenSymbolIsOfInterestWithDebuggingOnWine(string symbol)
    {
        _debuggerSettingsValue.Enable = true;
        _gameExecutionContext.IsWine.ReturnsForAnyArgs(true);
        _sut.HookMonoInitialization();

        string monoFileName = "mono-2.0-bdwgc.dll";
        byte[] monoFileNameBytes = Encoding.Unicode.GetBytes(monoFileName);
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).ReturnsForAnyArgs(symbolAddress);
        _win32.WhenForAnyArgs(x => x.GetModuleFileName(Arg.Any<HMODULE>(), Arg.Any<PWSTR>(), Arg.Any<uint>()))
            .Do(c => Marshal.Copy(monoFileNameBytes, 0, (nint)c.ArgAt<PWSTR>(1).Value, monoFileNameBytes.Length));

        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            new PCSTR(symbolPtr))!;

        result.Should().NotBe(symbolAddress);
        _win32.Received(1).GetModuleFileName(moduleHandle, Arg.Any<PWSTR>(), Arg.Any<uint>());
        _sdbWinePathTranslator.Received(1).Setup(monoFileName);
        _win32.Received(1).GetProcAddress(moduleHandle, symbolPtr);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_ConfiguresMonoCorrectly_WhenCalled()
    {
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        IntPtr receivedDomainNamePtr = nint.Zero;
        IntPtr receivedRuntimeVersionPtr = nint.Zero;
        int expectedReturn = Random.Shared.Next();
        string assemblyRootDir = "rootdir";
        string receivedMonoAssembliesPath = "";
        string expectedMonoAssembliesPath =
            $"{Path.Combine(_gameExecutionContext.GameDir, "UnityJitMonoBcl")};{assemblyRootDir}";
        IntPtr monoThreadCurrent = Random.Shared.Next();
        nint receivedMonoThreadSetMain = nint.Zero;
        (int argc, string[] argv) receivedArgs = default;
        nint receivedSetConfigDomain = nint.Zero;
        string receivedSetConfigPath = "";
        string receivedSetConfigFile = "";
        string? receivedConfigParse = "";
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _monoFunctions.JitInitVersion.Returns((domainName, runtimeVersion) =>
        {
            receivedDomainNamePtr = domainName;
            receivedRuntimeVersionPtr = runtimeVersion;
            return expectedReturn;
        });
        _monoFunctions.DomainSetConfig.Returns((domain, configPath, configFile) =>
        {
            receivedSetConfigDomain = domain;
            receivedSetConfigPath = configPath;
            receivedSetConfigFile = configFile;
        });
        _monoFunctions.ConfigParse.Returns(config => receivedConfigParse = config);
        _monoFunctions.ThreadCurrent.Returns(() => monoThreadCurrent);
        _monoFunctions.ThreadSetMain.Returns(thread => receivedMonoThreadSetMain = thread);
        _monoFunctions.AssemblyGetrootdir.Returns(() => assemblyRootDir);
        _monoFunctions.SetAssembliesPath.Returns(path => receivedMonoAssembliesPath = path);
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitInitVersionFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        IntPtr result = detour(domainNamePtr, runtimeVersionPtr);

        result.Should().Be(expectedReturn);
        receivedDomainNamePtr.Should().Be(domainNamePtr);
        receivedRuntimeVersionPtr.Should().Be(runtimeVersionPtr);
        receivedMonoAssembliesPath.Should().Be(expectedMonoAssembliesPath);
        receivedArgs.argc.Should().Be(0);
        receivedArgs.argv.Should().BeEmpty();
        receivedMonoThreadSetMain.Should().Be(monoThreadCurrent);
        receivedSetConfigDomain.Should().Be(result);
        receivedSetConfigPath.Should().Be(_gameExecutionContext.GameDir);
        receivedSetConfigFile.Should().Be($"{Environment.ProcessPath}.config");
        receivedConfigParse.Should().BeNull();
        _monoFunctions.ReceivedWithAnyArgs(1).AssemblyGetImage(Arg.Any<nint>());
        _monoFunctions.ReceivedWithAnyArgs(1).DomainAssemblyOpen(result, Arg.Any<string>());
        _monoFunctions.ReceivedWithAnyArgs(1).ClassFromName(Arg.Any<nint>(), Arg.Any<string>(), Arg.Any<string>());
        _monoFunctions.ReceivedWithAnyArgs(1)
            .ClassGetMethodFromName(Arg.Any<nint>(), Arg.Any<string>(), Arg.Any<int>());
        _monoFunctions.ReceivedWithAnyArgs(1).RuntimeInvoke(
            Arg.Any<nint>(),
            Arg.Any<nint>(),
            null,
            ref Arg.Any<nint>());
        _monoInitLifeCycleEvents.Received(1).Publish(Arg.Any<object>());
        _pltHooksManager.Hooks.Should()
            .NotContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.GetProcAddress)));

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_ConfiguresDebuggingCorrectly_WhenCalledAndDebugInitWasNotCalled()
    {
        _debuggerSettingsValue.Enable = true;
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] args) receivedArgs = default;
        (int argc, string[] argv) expectedArgs = GetArgsFromString(
            $"--debugger-agent=transport=dt_socket,server=y,address=" +
            $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
            ",suspend=n");
        IMonoFunctions.MonoDebugFormat receivedFormat = IMonoFunctions.MonoDebugFormat.MonoDebugFormatNone;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = format);
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitInitVersionFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        detour(domainNamePtr, runtimeVersionPtr);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);
        receivedFormat.Should().Be(IMonoFunctions.MonoDebugFormat.MonoDebugFormatMono);
        _playerConnectionDiscovery.Received(1)
            .StartDiscoveryWithOwnSocket(_debuggerSettingsValue.IpAddress, (ushort)_debuggerSettingsValue.Port!.Value);

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_ConfiguresDebuggingCorrectly_WhenCalledAfterDebugInit()
    {
        _debuggerSettingsValue.Enable = true;
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR jitInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        PCSTR debugInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_debug_init");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] args) receivedArgs = default;
        (int argc, string[] argv) expectedArgs = GetArgsFromString(
            $"--debugger-agent=transport=dt_socket,server=y,address=" +
            $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
            ",suspend=n");
        IMonoFunctions.MonoDebugFormat receivedFormat = IMonoFunctions.MonoDebugFormat.MonoDebugFormatNone;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = format);
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr jitInitDetourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitInitSymbolPtr)!;
        IntPtr debugInitDetourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            debugInitSymbolPtr)!;
        IMonoFunctions.JitInitVersionFn jitInitDetour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(jitInitDetourPtr);
        IMonoFunctions.DebugInitFn debugInitDetour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.DebugInitFn>(debugInitDetourPtr);

        debugInitDetour(IMonoFunctions.MonoDebugFormat.MonoDebugFormatMono);
        jitInitDetour(domainNamePtr, runtimeVersionPtr);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);
        receivedFormat.Should().Be(IMonoFunctions.MonoDebugFormat.MonoDebugFormatMono);
        _monoFunctions.Received(1).DebugInit(Arg.Any<IMonoFunctions.MonoDebugFormat>());
        _playerConnectionDiscovery.Received(1)
            .StartDiscoveryWithSendToHook(_debuggerSettingsValue.IpAddress, (ushort)_debuggerSettingsValue.Port!.Value);

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)jitInitSymbolPtr.Value);
        Marshal.FreeHGlobal((nint)debugInitSymbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_CallsOriginalWithoutConfiguringMonoTwice_WhenCalledTwice()
    {
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        IntPtr receivedDomainNamePtr = nint.Zero;
        IntPtr receivedRuntimeVersionPtr = nint.Zero;
        int expectedReturn = Random.Shared.Next();

        _monoFunctions.JitInitVersion.Returns((domainName, runtimeVersion) =>
        {
            receivedDomainNamePtr = domainName;
            receivedRuntimeVersionPtr = runtimeVersion;
            return expectedReturn;
        });
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitInitVersionFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        detour(domainNamePtr, runtimeVersionPtr);
        IntPtr result = detour(domainNamePtr, runtimeVersionPtr);

        result.Should().Be(expectedReturn);
        receivedDomainNamePtr.Should().Be(domainNamePtr);
        receivedRuntimeVersionPtr.Should().Be(runtimeVersionPtr);
        _monoInitLifeCycleEvents.Received(1).Publish(Arg.Any<object>());

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_Logs_WhenOpeningEntryPointAssemblyFailed()
    {
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => nint.Zero);
        IntPtr detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitInitVersionFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        IntPtr result = detour(domainNamePtr, runtimeVersionPtr);

        _monoFunctions.ReceivedWithAnyArgs(1).DomainAssemblyOpen(result, Arg.Any<string>());
        _monoFunctions.DidNotReceiveWithAnyArgs().RuntimeInvoke(
            Arg.Any<nint>(),
            Arg.Any<nint>(),
            null,
            ref Arg.Any<nint>());
        _logger.Collector.GetSnapshot().Should().ContainSingle(log => log.Level == LogLevel.Critical);

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginal_WhenDebuggerIsDisabled()
    {
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] argv) originalArgs = GetArgsFromString("stuff things");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitParseOptionsFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(originalArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginalWithDnSpyArgs_WhenDnSpyEnvVarIsSetAndDebuggerIsDisabled()
    {
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] argv) originalArgs = GetArgsFromString("stuff things");
        string dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        ushort dnSpyPort = (ushort)Random.Shared.Next();
        string dnSpyEnvVar = $"things,address={dnSpyIp}:{dnSpyPort},stuff";
        (int argc, string[] argv) expectedArgs = GetArgsFromString($"stuff things {dnSpyEnvVar}");
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", dnSpyEnvVar);
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitParseOptionsFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginalWithDebugArgs_WhenDebuggerIsEnabled(bool withSuspend)
    {
        _debuggerSettingsValue.Enable = true;
        _debuggerSettingsValue.SuspendOnBoot = withSuspend;
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] argv) originalArgs = GetArgsFromString("stuff things");
        string debugArgs = $"--debugger-agent=transport=dt_socket,server=y,address=" +
                           $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
                           $"{(withSuspend ? "" : ",suspend=n")}";
        (int argc, string[] argv) expectedArgs = GetArgsFromString($"stuff things {debugArgs}");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitParseOptionsFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void
        MonoJitParseOptionsDetour_OverridesDebugArgsWithDnSpyDebugArgs_WhenDnSpyEnvVarIsSetAndDebuggerIsEnabled()
    {
        _debuggerSettingsValue.Enable = true;
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] argv) originalArgs = GetArgsFromString("stuff things");
        string dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        ushort dnSpyPort = (ushort)Random.Shared.Next();
        string dnSpyEnvVar = $"things,address={dnSpyIp}:{dnSpyPort},stuff";
        (int argc, string[] argv) expectedArgs = GetArgsFromString($"stuff things {dnSpyEnvVar}");
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", dnSpyEnvVar);
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.JitParseOptionsFn detour =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginal_WhenCalledAfterMonoJitInitWhileDebuggerIsEnabled()
    {
        _debuggerSettingsValue.Enable = true;
        _sut.HookMonoInitialization();

        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        IntPtr runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        PCSTR jitParseOptionSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        PCSTR jitInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] argv) originalArgs = GetArgsFromString("stuff things");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        IntPtr detourJitParseOptionsPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitParseOptionSymbolPtr)!;
        IntPtr detourJitInitPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitInitSymbolPtr)!;
        IMonoFunctions.JitParseOptionsFn detourJitParseOptions =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(detourJitParseOptionsPtr);
        IMonoFunctions.JitInitVersionFn detourJitInit =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourJitInitPtr);

        detourJitInit(domainNamePtr, runtimeVersionPtr);
        detourJitParseOptions(originalArgs.argc, originalArgs.argv);
        receivedArgs.Should().BeEquivalentTo(originalArgs);

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)jitParseOptionSymbolPtr.Value);
        Marshal.FreeHGlobal((nint)jitInitSymbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoDebugInitDetour_CallsOriginal_WhenCalled()
    {
        _sut.HookMonoInitialization();

        PCSTR symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_debug_init");
        FARPROC symbolAddress = (FARPROC)Random.Shared.Next();
        HMODULE moduleHandle = (HMODULE)Random.Shared.Next();
        IMonoFunctions.MonoDebugFormat debugFormat = (IMonoFunctions.MonoDebugFormat)Random.Shared.Next(3);
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        int receivedFormat = -1;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = (int)format);
        IntPtr result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        IMonoFunctions.DebugInitFn detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.DebugInitFn>(result);

        detour(debugFormat);

        receivedFormat.Should().Be((int)debugFormat);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    private (int argc, string[] argv) GetArgsFromString(string args)
    {
        string[] splitArgs = args.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return (splitArgs.Length, splitArgs);
    }
}