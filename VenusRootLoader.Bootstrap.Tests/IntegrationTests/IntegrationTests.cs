using AwesomeAssertions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Bootstrap.Tests.IntegrationTests;

[Collection(nameof(IntegrationTests))]
public sealed class IntegrationTests : IDisposable
{
    private const string SilentPlayerArguments = "-hidewindow -nographics -batchmode -no-dialogs";

    private static readonly string ReleaseBuildInstallPath = "TestInstall";
    private static readonly string DevBuildInstallPath =  "TestInstallDevBuild";

    private static string TestExecutable(string buildPath) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Directory.GetCurrentDirectory(), buildPath, "VenusRootLoaderTestProject.exe")
            : "wine";

    private static string TestArguments => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? SilentPlayerArguments
        : $"./VenusRootLoaderTestProject.exe {SilentPlayerArguments}";

    public IntegrationTests()
    {
        CleanupTestData(ReleaseBuildInstallPath);
        CleanupTestData(DevBuildInstallPath);
    }

    private static void CleanupTestData(string rootPath)
    {
        if (Directory.Exists(Path.Combine(rootPath, "Logs")))
            Directory.Delete(Path.Combine(rootPath, "Logs"), true);
        if (File.Exists(Path.Combine(rootPath, "VenusRootLoader", "data.unity3d.modified")))
            File.Delete(Path.Combine(rootPath, "VenusRootLoader", "data.unity3d.modified"));
        if (File.Exists(Path.Combine(rootPath, "VenusRootLoader", "data.unity3d.modified.uncompressed")))
            File.Delete(Path.Combine(rootPath, "VenusRootLoader", "data.unity3d.modified.uncompressed"));
    }

    public static List<object[]> VrlEnabledTestData =>
    [
        [ReleaseBuildInstallPath, false, false],
        [ReleaseBuildInstallPath, false, true],
        [ReleaseBuildInstallPath, true, false],
        [ReleaseBuildInstallPath, true, true],
        [DevBuildInstallPath, false, false],
        [DevBuildInstallPath, false, true],
        [DevBuildInstallPath, true, false],
        [DevBuildInstallPath, true, true]
    ];

    public static List<object[]> VrlDisabledTestData =>
    [
        [ReleaseBuildInstallPath],
        [DevBuildInstallPath],
    ];

    [Theory(Skip = "Integration tests are flaky, need to recheck")]
    [MemberData(nameof(VrlEnabledTestData))]
    public void Bootstrap_BootsGameSuccessfully_WhenVrlIsEnabled(
        string buildPath,
        bool debugMode,
        bool skipSplashScreen)
    {
        Process proc = new()
        {
            StartInfo = new()
            {
                Arguments = TestArguments,
                EnvironmentVariables =
                {
                    ["VRL_GLOBAL_DISABLE"] = "false",
                    ["VRL_ENABLE_CONSOLE_LOGS"] = "false",
                    ["INCLUDE_UNITY_LOGS"] = "true",
                    ["VRL_ENABLE_FILES_LOGS"] = "true",
                    ["VRL_DEBUGGER_SUSPEND_BOOT"] = "false",
                    ["VRL_DEBUGGER_ENABLE"] = debugMode.ToString(),
                    ["VRL_SKIP_UNITY_SPLASHSCREEN"] = skipSplashScreen.ToString(),
                    ["DNSPY_UNITY_DBG2"] = null
                },
                FileName = TestExecutable(buildPath),
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), buildPath)
            }
        };

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            SetupWineEnvironment(proc);

        proc.Start();
        proc.WaitForExit(TimeSpan.FromSeconds(10));
        proc.Kill();
        foreach (var file in Directory.GetFiles(Path.Combine(buildPath, "VenusRootLoader")))
        {
            TestContext.Current.TestOutputHelper!.WriteLine(file);
        }
        string[] logs = File.ReadAllLines(Path.Combine(buildPath, "Logs", "latest.log"));
        try
        {
            logs.Should().NotContainMatch("*[!]*");
            logs.Should().NotContainMatch("*[E]*");
            logs.Should().NotContainMatch("*[W]*");
            if (skipSplashScreen)
                logs.Should().Contain(l => l.Contains("Redirecting game bundle"));
            else
                logs.Should().NotContain(l => l.Contains("Redirecting game bundle"));
            if (debugMode)
                logs.Should().Contain(l => l.Contains("Adding jit options"));
            else
                logs.Should().NotContain(l => l.Contains("Adding jit options"));

            logs.Should().ContainSingle(l => l.EndsWith("<Game started successfully>"));
        }
        catch (Exception)
        {
            TestContext.Current.TestOutputHelper!.WriteLine("Full VenusRootLoader logs:");
            foreach (string log in logs)
                TestContext.Current.TestOutputHelper!.WriteLine(log);
            throw;
        }
    }

    [Theory(Skip = "Integration tests are flaky, need to recheck")]
    [MemberData(nameof(VrlDisabledTestData))]
    public void Bootstrap_DoesNothing_WhenVrlIsDisabled(string buildPath)
    {
        Process proc = new()
        {
            StartInfo = new()
            {
                Arguments = TestArguments,
                EnvironmentVariables =
                {
                    ["VRL_GLOBAL_DISABLE"] = "true",
                    ["VRL_ENABLE_CONSOLE_LOGS"] = "false",
                    ["VRL_ENABLE_FILES_LOGS"] = "true"
                },
                FileName = TestExecutable(buildPath),
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), buildPath)
            }
        };

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            SetupWineEnvironment(proc);

        proc.Start();
        proc.WaitForExit(TimeSpan.FromSeconds(7));
        proc.Kill();
        Directory.Exists("./TestInstall/Logs").Should().BeFalse();
    }

    private static void SetupWineEnvironment(Process proc)
    {
        proc.StartInfo.EnvironmentVariables["WINEDLLOVERRIDES"] = "winhttp.dll=n,b";
    }

    public void Dispose()
    {
        CleanupTestData(ReleaseBuildInstallPath);
        CleanupTestData(DevBuildInstallPath);
    }
}