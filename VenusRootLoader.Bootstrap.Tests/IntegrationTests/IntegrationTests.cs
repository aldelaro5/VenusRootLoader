using AwesomeAssertions;
using System.Diagnostics;

namespace VenusRootLoader.Bootstrap.Tests.IntegrationTests;

[Collection(nameof(IntegrationTests))]
public class IntegrationTests : IDisposable
{
    private const string SilentPlayerArguments = "-hidewindow -nographics -batchmode -no-dialogs";
    private const string ReleaseBuildInstallPath = "./TestInstall";
    private const string DevBuildInstallPath = "./TestInstallDevBuild";

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

    [Theory]
    [InlineData(ReleaseBuildInstallPath, false, false)]
    [InlineData(ReleaseBuildInstallPath, false, true)]
    [InlineData(ReleaseBuildInstallPath, true, false)]
    [InlineData(ReleaseBuildInstallPath, true, true)]
    [InlineData(DevBuildInstallPath, false, false)]
    [InlineData(DevBuildInstallPath, false, true)]
    [InlineData(DevBuildInstallPath, true, false)]
    [InlineData(DevBuildInstallPath, true, true)]
    public void Bootstrap_BootsGameSuccessfully_WhenVrlIsEnabled(
        string buildPath,
        bool debugMode,
        bool skipSplashScreen)
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
                    ["VRL_SKIP_UNITY_SPLASHSCREEN"] = skipSplashScreen.ToString(),
                    ["DNSPY_UNITY_DBG2"] = null
                },
                FileName = "wine",
                UseShellExecute = false,
                WorkingDirectory = buildPath
            }
        };
        proc.Start();
        proc.WaitForExit(skipSplashScreen ? TimeSpan.FromSeconds(3) : TimeSpan.FromSeconds(7));
        proc.Kill();
        string[] logs = File.ReadAllLines(Path.Combine(buildPath, "Logs", "latest.log"));
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

    [Theory]
    [InlineData(ReleaseBuildInstallPath)]
    [InlineData(DevBuildInstallPath)]
    public void Bootstrap_DoesNothing_WhenVrlIsDisabled(string buildPath)
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
                WorkingDirectory = buildPath
            }
        };
        proc.Start();
        proc.WaitForExit(TimeSpan.FromSeconds(7));
        proc.Kill();
        Directory.Exists("./TestInstall/Logs").Should().BeFalse();
    }

    public void Dispose()
    {
        CleanupTestData(ReleaseBuildInstallPath);
        CleanupTestData(DevBuildInstallPath);
    }
}