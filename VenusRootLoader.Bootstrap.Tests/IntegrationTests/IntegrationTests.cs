using AwesomeAssertions;
using System.Diagnostics;

namespace VenusRootLoader.Bootstrap.Tests.IntegrationTests;

[Collection(nameof(IntegrationTests))]
public class IntegrationTests : IDisposable
{
    private const string SilentPlayerArguments = "-hidewindow -nographics -batchmode -no-dialogs";

    public IntegrationTests()
    {
        CleanupTestData();
    }

    private static void CleanupTestData()
    {
        if (Directory.Exists("./TestInstall/Logs"))
            Directory.Delete("./TestInstall/Logs", true);
        if (File.Exists("./TestInstall/VenusRootLoader/data.unity3d.modified"))
            File.Delete("./TestInstall/VenusRootLoader/data.unity3d.modified");
        if (File.Exists("./TestInstall/VenusRootLoader/data.unity3d.modified.uncompressed"))
            File.Delete("./TestInstall/VenusRootLoader/data.unity3d.modified.uncompressed");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Bootstrap_BootsGameSuccessfully_WhenVrlIsEnabled(bool debugMode, bool skipSplashScreen)
    {
        Process proc = new()
        {
            StartInfo = new()
            {
                Arguments = "./VenusRootLoaderTestProject.exe " + SilentPlayerArguments,
                EnvironmentVariables =
                {
                    ["VRL_GLOBAL_DISABLE"] = "false",
                    ["VRL_ENABLE_CONSOLE_LOGS"] = "false",
                    ["INCLUDE_UNITY_LOGS"] = "true",
                    ["VRL_ENABLE_FILES_LOGS"] = "true",
                    ["VRL_DEBUGGER_SUSPEND_BOOT"] = "false",
                    ["VRL_DEBUGGER_ENABLE"] = debugMode.ToString(),
                    ["VRL_SKIP_UNITY_SPLASHSCREEN"] = skipSplashScreen.ToString()
                },
                FileName = "wine",
                UseShellExecute = false,
                WorkingDirectory = "TestInstall"
            }
        };
        proc.Start();
        proc.WaitForExit(skipSplashScreen ? TimeSpan.FromSeconds(3) : TimeSpan.FromSeconds(7));
        proc.Kill();
        string[] logs = File.ReadAllLines("./TestInstall/Logs/latest.log");
        logs.Should().NotContainMatch("*[!]*");
        logs.Should().NotContainMatch("*[E]*");
        logs.Should().NotContainMatch("*[W]*");
        if (skipSplashScreen)
            logs.Should().Contain(l => l.Contains("Redirecting game bundle"));
        else
            logs.Should().NotContain(l => l.Contains("Redirecting game bundle"));
        if (debugMode)
            logs.Should().Contain(l => l.Contains("Initialising Mono debugger"));
        else
            logs.Should().NotContain(l => l.Contains("Initialising Mono debugger"));
        logs.Should().ContainSingle(l => l.EndsWith("<Game started successfully>"));
    }

    [Fact]
    public void Bootstrap_DoesNothing_WhenVrlIsDisabled()
    {
        Process proc = new()
        {
            StartInfo = new()
            {
                Arguments = "./VenusRootLoaderTestProject.exe " + SilentPlayerArguments,
                EnvironmentVariables =
                {
                    ["VRL_GLOBAL_DISABLE"] = "true",
                    ["VRL_ENABLE_CONSOLE_LOGS"] = "false",
                    ["VRL_ENABLE_FILES_LOGS"] = "true"
                },
                FileName = "wine",
                UseShellExecute = false,
                WorkingDirectory = "TestInstall"
            }
        };
        proc.Start();
        proc.WaitForExit(TimeSpan.FromSeconds(7));
        proc.Kill();
        Directory.Exists("./TestInstall/Logs").Should().BeFalse();
    }

    public void Dispose()
    {
        CleanupTestData();
    }
}