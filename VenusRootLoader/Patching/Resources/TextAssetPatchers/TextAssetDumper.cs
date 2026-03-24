using System.IO.Abstractions;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface ITextAssetDumper
{
    void DumpTextAssetContent(string dataSubpath, string content);
}

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

        string dumpPath = _fileSystem.Path.Combine(_gameExecutionContext.GameDir, "DataDump", dumpSubpath);
        string dumpPathWithExtension = _fileSystem.Path.ChangeExtension(dumpPath, "txt");
        _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(dumpPathWithExtension)!);
        _fileSystem.File.WriteAllText(dumpPathWithExtension, content);
    }
}