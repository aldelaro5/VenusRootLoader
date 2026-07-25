using System.IO.Abstractions;
using UnityEngine;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

/// <summary>
/// A service for easy dumping of a patched <see cref="TextAsset"/>'s content on disk for diagnosis purposes.
/// </summary>
internal interface ITextAssetDumper
{
    /// <summary>
    /// Dumps the content of a <see cref="TextAsset"/> using a <c>.txt</c> extension at a dump directory/<paramref name="dataSubpath"/>.
    /// </summary>
    /// <param name="dataSubpath">The path to dump the content relative to the dumping directory.</param>
    /// <param name="content">The content to dump to a file.</param>
    void DumpTextAssetContent(string dataSubpath, string content);
}

/// <inheritdoc/>
internal sealed class TextAssetDumper : ITextAssetDumper
{
    private readonly IFileSystem _fileSystem;
    private readonly GameExecutionContext _gameExecutionContext;

    public TextAssetDumper(IFileSystem fileSystem, GameExecutionContext gameExecutionContext)
    {
        _fileSystem = fileSystem;
        _gameExecutionContext = gameExecutionContext;
    }

    public void DumpTextAssetContent(string dataSubpath, string content)
    {
        string dumpSubpath;
        if (dataSubpath.LastIndexOf('/') != -1)
        {
            string directory = dataSubpath[..(dataSubpath.LastIndexOf('/') + 1)];
            string name = dataSubpath[(dataSubpath.LastIndexOf('/') + 1)..];
            dumpSubpath = directory.ToLower() + name;
        }
        else
        {
            dumpSubpath = dataSubpath;
        }

        // We're hardcoding this for now since this is a Trace logging level only feature.
        string dumpPath = _fileSystem.Path.Combine(_gameExecutionContext.GameDir, "DataDump", dumpSubpath);
        string dumpPathWithExtension = _fileSystem.Path.ChangeExtension(dumpPath, "txt");
        _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(dumpPathWithExtension)!);
        _fileSystem.File.WriteAllText(dumpPathWithExtension, content);
    }
}