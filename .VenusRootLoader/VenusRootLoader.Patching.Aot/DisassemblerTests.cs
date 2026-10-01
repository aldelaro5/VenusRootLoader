using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using System.Reflection.Metadata;

namespace VenusRootLoader.Patching.Aot;

internal sealed class DisassemblerTests
{
    internal static void DisassembleTypes(string filePath, DirectoryInfo outputDirectory)
    {
        PEFile peFile = new(filePath);
        if (!Directory.Exists(outputDirectory.FullName))
            Directory.CreateDirectory(outputDirectory.FullName);

        List<TypeDefinitionHandle> types = peFile.Metadata.TypeDefinitions
            .Where(x => x.GetFullTypeName(peFile.Metadata).Name.Contains("<EnemyAction"))
            .Concat(
            [
                peFile.Metadata.TypeDefinitions
                    .Single(x => x.GetFullTypeName(peFile.Metadata).Name.Contains("<DoAction>"))
            ])
            .ToList();

        foreach (TypeDefinitionHandle type in types)
        {
            string fileName = Path.Combine(
                outputDirectory.FullName,
                type.GetFullTypeName(peFile.Metadata).Name
                    .Replace("<", "_")
                    .Replace(">", "_") + ".il");

            using StreamWriter streamWriter = File.CreateText(fileName);
            ReflectionDisassembler disassembler = new(new PlainTextOutput(streamWriter), CancellationToken.None);
            disassembler.ShowRawRVAOffsetAndBytes = false;
            disassembler.DisassembleType(peFile, type);
        }
    }
}