using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Abstractions;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// A service that invokes the AOT patcher. This needs to be done in the bootstrap because we need to redirect the assembly
/// load so the game doesn't load its original assembly.
/// </summary>
public interface IAssemblyCSharpAotPatcher
{
    /// <summary>
    /// Invokes VenusRootLoader.Patching.Aot.exe on the original game assembly to patch it and write it in a subdirectory
    /// of the base directory.
    /// </summary>
    void PatchAssemblyCSharp();
}

public sealed class AssemblyCSharpAotPatcher : IAssemblyCSharpAotPatcher
{
    private readonly ILogger<AssemblyCSharpAotPatcher> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IBootstrapEnvironment _bootstrapEnvironment;
    private readonly IGameExecutionContext _gameExecutionContext;

    public AssemblyCSharpAotPatcher(
        ILogger<AssemblyCSharpAotPatcher> logger,
        IFileSystem fileSystem,
        IBootstrapEnvironment bootstrapEnvironment,
        IGameExecutionContext gameExecutionContext)
    {
        _fileSystem = fileSystem;
        _gameExecutionContext = gameExecutionContext;
        _logger = logger;
        _bootstrapEnvironment = bootstrapEnvironment;
    }

    public void PatchAssemblyCSharp()
    {
        string aotPatcherOutputDirectory = _fileSystem.Path.Combine(_bootstrapEnvironment.BasePath, "GameAssembly");
        if (!_fileSystem.Directory.Exists(aotPatcherOutputDirectory))
            _fileSystem.Directory.CreateDirectory(aotPatcherOutputDirectory);

        string assemblyPath = _fileSystem.Path.Combine(_gameExecutionContext.DataDir, "Managed", Constants.GameAssemblyFileName);
        string aotPatcherOutputPath = _fileSystem.Path.Combine(aotPatcherOutputDirectory, Constants.GameAssemblyFileName);
        string aotPatcherPath = _fileSystem.Path.Combine(
            _bootstrapEnvironment.BasePath,
            "VenusRootLoader",
            "VenusRootLoader.Patching.Aot.exe");

        using MemoryStream outputStream = new();
        using MemoryStream errorStream = new();
        
        using Process aotPatcherProcess = new();
        aotPatcherProcess.StartInfo.FileName = aotPatcherPath;
        aotPatcherProcess.StartInfo.Arguments = $"\"{assemblyPath}\" \"{aotPatcherOutputPath}\"";
        aotPatcherProcess.StartInfo.UseShellExecute = false;
        aotPatcherProcess.StartInfo.RedirectStandardOutput = true;
        aotPatcherProcess.StartInfo.RedirectStandardError = true;
        aotPatcherProcess.OutputDataReceived += OnAotPatcherOutput;
        aotPatcherProcess.ErrorDataReceived += OnAotPatcherError;
        
        _logger.LogInformation("AOT Patching the game assembly located at {assemblyPath}", assemblyPath);

        aotPatcherProcess.Start();
        aotPatcherProcess.BeginOutputReadLine();
        aotPatcherProcess.BeginErrorReadLine();
        aotPatcherProcess.WaitForExit();
        aotPatcherProcess.OutputDataReceived -= OnAotPatcherOutput;
        aotPatcherProcess.ErrorDataReceived -= OnAotPatcherError;

        if (aotPatcherProcess.ExitCode != 0)
            throw new Exception("The AOT patcher failed with exit code " + aotPatcherProcess.ExitCode);

        _logger.LogInformation("AOT patching succeeded, patched assembly was written to {aotPatcherOutputPath}", aotPatcherOutputPath);
    }

    private void OnAotPatcherOutput(object sender, DataReceivedEventArgs e)
    {
        // Null means the stream ended, we don't need to do anything in such case.
        if (e.Data == null)
            return;
        _logger.LogInformation(e.Data);
    }

    private void OnAotPatcherError(object sender, DataReceivedEventArgs e)
    {
        // Null means the stream ended, we don't need to do anything in such case.
        if (e.Data == null)
            return;
        _logger.LogError(e.Data);
    }
}