using AwesomeAssertions;
using Microsoft.Extensions.Hosting;
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
using Windows.Win32.Foundation;

namespace VenusRootLoader.Bootstrap.Tests.Mono;

[Collection(nameof(MonoInitializerTests))]
public class MonoInitializerTests
{
    private readonly FakeLogger<MonoInitializer> _logger = new FakeLogger<MonoInitializer>();

    private readonly IOptions<MonoDebuggerSettings>
        _debuggerSettings = Substitute.For<IOptions<MonoDebuggerSettings>>();

    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();

    private readonly IPlayerConnectionDiscovery _playerConnectionDiscovery =
        Substitute.For<IPlayerConnectionDiscovery>();

    private readonly ISdbWinePathTranslator _sdbWinePathTranslator = Substitute.For<ISdbWinePathTranslator>();
    private readonly IMonoFunctions _monoFunctions = Substitute.For<IMonoFunctions>();
    private readonly IGameLifecycleEvents _gameLifecycleEvents = Substitute.For<IGameLifecycleEvents>();
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly MockFileSystem _fileSystem = new();

    private readonly MonoDebuggerSettings _debuggerSettingsValue = new()
    {
        Enable = false,
        IpAddress = "0.0.0.0",
        Port = 55555,
        SuspendOnBoot = false
    };

    private GameExecutionContext _gameExecutionContext = new()
    {
        LibraryHandle = 0,
        GameDir = "Game",
        DataDir = "",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };

    public MonoInitializerTests()
    {
        _debuggerSettings.Value.Returns(_debuggerSettingsValue);
        _hostEnvironment.ContentRootPath.Returns("root");
    }

    private void StartService()
    {
        var sut = new MonoInitializer(
            _logger,
            _pltHooksManager,
            _gameExecutionContext,
            _debuggerSettings,
            _playerConnectionDiscovery,
            _sdbWinePathTranslator,
            _gameLifecycleEvents,
            _hostEnvironment,
            _win32,
            _fileSystem,
            _monoFunctions);
        sut.StartAsync(TestContext.Current.CancellationToken);
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", null);
    }

    [Fact]
    public void StartAsync_InstallsGetProcAddressHook_WhenCalled()
    {
        StartService();

        _pltHooksManager.Hooks.Should()
            .ContainKey((_gameExecutionContext.UnityPlayerDllFileName, nameof(_win32.GetProcAddress)));
    }

    [Fact]
    public unsafe void GetProcAddressHook_ReturnOriginalResult_WhenSymbolIsNotOfInterest()
    {
        StartService();

        var symbol = "SomeFunction";
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);

        var result = (nint)_pltHooksManager.SimulateHook(
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
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);

        var result = (nint)_pltHooksManager.SimulateHook(
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
        _gameExecutionContext = new()
        {
            LibraryHandle = 0,
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = true
        };
        StartService();

        var monoFileName = "mono-2.0-bdwgc.dll";
        var monoFileNameBytes = Encoding.Unicode.GetBytes(monoFileName);
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi(symbol);
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).ReturnsForAnyArgs(symbolAddress);
        _win32.WhenForAnyArgs(x => x.GetModuleFileName(Arg.Any<HMODULE>(), Arg.Any<PWSTR>(), Arg.Any<uint>()))
            .Do(c => Marshal.Copy(monoFileNameBytes, 0, (nint)c.ArgAt<PWSTR>(1).Value, monoFileNameBytes.Length));

        var result = (nint)_pltHooksManager.SimulateHook(
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
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var receivedDomainNamePtr = nint.Zero;
        var receivedRuntimeVersionPtr = nint.Zero;
        var expectedReturn = Random.Shared.Next();
        var assemblyRootDir = "rootdir";
        var receivedMonoAssembliesPath = "";
        var expectedMonoAssembliesPath =
            $"{Path.Combine(_hostEnvironment.ContentRootPath, "UnityJitMonoBcl")};{assemblyRootDir}";
        var monoThreadCurrent = (nint)Random.Shared.Next();
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
        var detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        var result = detour(domainNamePtr, runtimeVersionPtr);

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
        _gameLifecycleEvents.Received(1).Publish(Arg.Any<object>());
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
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] args) receivedArgs = default;
        var expectedArgs = GetArgsFromString(
            $"--debugger-agent=transport=dt_socket,server=y,address=" +
            $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
            ",suspend=n");
        var receivedFormat = IMonoFunctions.MonoDebugFormat.MonoDebugFormatNone;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = format);
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

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
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var jitInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var debugInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_debug_init");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        (int argc, string[] args) receivedArgs = default;
        var expectedArgs = GetArgsFromString(
            $"--debugger-agent=transport=dt_socket,server=y,address=" +
            $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
            ",suspend=n");
        var receivedFormat = IMonoFunctions.MonoDebugFormat.MonoDebugFormatNone;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = format);
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var jitInitDetourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitInitSymbolPtr)!;
        var debugInitDetourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            debugInitSymbolPtr)!;
        var jitInitDetour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(jitInitDetourPtr);
        var debugInitDetour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.DebugInitFn>(debugInitDetourPtr);

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
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var receivedDomainNamePtr = nint.Zero;
        var receivedRuntimeVersionPtr = nint.Zero;
        var expectedReturn = Random.Shared.Next();

        _monoFunctions.JitInitVersion.Returns((domainName, runtimeVersion) =>
        {
            receivedDomainNamePtr = domainName;
            receivedRuntimeVersionPtr = runtimeVersion;
            return expectedReturn;
        });
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => Random.Shared.Next());
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        detour(domainNamePtr, runtimeVersionPtr);
        var result = detour(domainNamePtr, runtimeVersionPtr);

        result.Should().Be(expectedReturn);
        receivedDomainNamePtr.Should().Be(domainNamePtr);
        receivedRuntimeVersionPtr.Should().Be(runtimeVersionPtr);
        _gameLifecycleEvents.Received(1).Publish(Arg.Any<object>());

        Marshal.FreeHGlobal(domainNamePtr);
        Marshal.FreeHGlobal(runtimeVersionPtr);
        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitInitDetour_Logs_WhenOpeningEntryPointAssemblyFailed()
    {
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var moduleHandle = (HMODULE)Random.Shared.Next();
        _monoFunctions.DomainAssemblyOpen.Returns((_, _) => nint.Zero);
        var detourPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourPtr);

        var result = detour(domainNamePtr, runtimeVersionPtr);

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
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var originalArgs = GetArgsFromString("stuff things");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(originalArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginalWithDnSpyArgs_WhenDnSpyEnvVarIsSetAndDebuggerIsDisabled()
    {
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var originalArgs = GetArgsFromString("stuff things");
        var dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var dnSpyPort = (ushort)Random.Shared.Next();
        var dnSpyEnvVar = $"things,address={dnSpyIp}:{dnSpyPort},stuff";
        var expectedArgs = GetArgsFromString($"stuff things {dnSpyEnvVar}");
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", dnSpyEnvVar);
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

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
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var originalArgs = GetArgsFromString("stuff things");
        var debugArgs = $"--debugger-agent=transport=dt_socket,server=y,address=" +
                        $"{_debuggerSettings.Value.IpAddress}:{_debuggerSettings.Value.Port}" +
                        $"{(withSuspend ? "" : ",suspend=n")}";
        var expectedArgs = GetArgsFromString($"stuff things {debugArgs}");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void
        MonoJitParseOptionsDetour_OverridesDebugArgsWithDnSpyDebugArgs_WhenDnSpyEnvVarIsSetAndDebuggerIsEnabled()
    {
        _debuggerSettingsValue.Enable = true;
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var originalArgs = GetArgsFromString("stuff things");
        var dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var dnSpyPort = (ushort)Random.Shared.Next();
        var dnSpyEnvVar = $"things,address={dnSpyIp}:{dnSpyPort},stuff";
        var expectedArgs = GetArgsFromString($"stuff things {dnSpyEnvVar}");
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", dnSpyEnvVar);
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(result);

        detour(originalArgs.argc, originalArgs.argv);

        receivedArgs.Should().BeEquivalentTo(expectedArgs);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    [Fact]
    public unsafe void MonoJitParseOptionsDetour_CallsOriginal_WhenCalledAfterMonoJitInitWhileDebuggerIsEnabled()
    {
        _debuggerSettingsValue.Enable = true;
        StartService();

        var domainNamePtr = Marshal.StringToHGlobalAnsi("Unity Root Domain");
        var runtimeVersionPtr = Marshal.StringToHGlobalAnsi("v4.0.30319");
        var jitParseOptionSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_parse_options");
        var jitInitSymbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_jit_init_version");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var originalArgs = GetArgsFromString("stuff things");
        (int argc, string[] args) receivedArgs = default;
        _monoFunctions.JitParseOptions.Returns((argc, argv) => receivedArgs = ((int)argc, argv));
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var detourJitParseOptionsPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitParseOptionSymbolPtr)!;
        var detourJitInitPtr = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            jitInitSymbolPtr)!;
        var detourJitParseOptions =
            Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitParseOptionsFn>(detourJitParseOptionsPtr);
        var detourJitInit = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.JitInitVersionFn>(detourJitInitPtr);

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
        StartService();

        var symbolPtr = (PCSTR)(byte*)Marshal.StringToHGlobalAnsi("mono_debug_init");
        var symbolAddress = (FARPROC)Random.Shared.Next();
        var moduleHandle = (HMODULE)Random.Shared.Next();
        var debugFormat = (IMonoFunctions.MonoDebugFormat)Random.Shared.Next(3);
        _win32.GetProcAddress(Arg.Any<HMODULE>(), Arg.Any<PCSTR>()).Returns(symbolAddress);
        var receivedFormat = -1;
        _monoFunctions.DebugInit.Returns(format => receivedFormat = (int)format);
        var result = (nint)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.GetProcAddress),
            moduleHandle,
            symbolPtr)!;
        var detour = Marshal.GetDelegateForFunctionPointer<IMonoFunctions.DebugInitFn>(result);

        detour(debugFormat);

        receivedFormat.Should().Be((int)debugFormat);

        Marshal.FreeHGlobal((nint)symbolPtr.Value);
    }

    private (int argc, string[] argv) GetArgsFromString(string args)
    {
        var splitArgs = args.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return (splitArgs.Length, splitArgs);
    }
}