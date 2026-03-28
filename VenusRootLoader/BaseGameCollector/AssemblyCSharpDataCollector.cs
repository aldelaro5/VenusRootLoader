using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;

namespace VenusRootLoader.BaseGameCollector;

/// <summary>
/// This service allows collectors to perform complex query to the game's assembly on disk.
/// </summary>
internal interface IAssemblyCSharpDataCollector
{
    /// <summary>
    /// Reads an optimized out int array from the <c>&lt;PrivateImplementationDetails&gt;</c> type which typically means
    /// the array was originally hardcoded in the game's codebase.
    /// </summary>
    /// <param name="field">The field inside <c>&lt;PrivateImplementationDetails&gt;</c> to read the array from.</param>
    /// <returns>The array read.</returns>
    int[] ReadIntArrayFromPrivateImplementationDetailField(FieldInfo field);
}

/// <inheritdoc/>
internal sealed class AssemblyCSharpDataCollector : IAssemblyCSharpDataCollector
{
    private class AssemblyData
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        internal required AssemblyDefinition AssemblyDefinition { get; init; }
        internal required TypeDefinition PrivateImplementationDetailType { get; init; }
    }

    private readonly GameExecutionContext _executionContext;
    private readonly IFileSystem _fileSystem;

    private AssemblyData? _assemblyData;

    public AssemblyCSharpDataCollector(GameExecutionContext executionContext, IFileSystem fileSystem)
    {
        _executionContext = executionContext;
        _fileSystem = fileSystem;
    }

    public int[] ReadIntArrayFromPrivateImplementationDetailField(FieldInfo field)
    {
        if (_assemblyData is null)
            InitialiseAssemblyData();

        BinaryStreamReader reader = _assemblyData.PrivateImplementationDetailType.Fields
            .Single(f => f.Name == field.Name)
            .FieldRva!
            .ToReference()
            .CreateReader();

        List<int> data = new();
        while (reader.RemainingLength > 0)
            data.Add(reader.ReadInt32());

        return data.ToArray();
    }

    [MemberNotNull(nameof(_assemblyData))]
    private void InitialiseAssemblyData()
    {
        string assemblyPath = _fileSystem.Path.Combine(_executionContext.DataDir, "Managed", "Assembly-CSharp.dll");
        AssemblyDefinition assemblyDefinition = AssemblyDefinition.FromFile(assemblyPath);
        TypeDefinition type = assemblyDefinition.ManifestModule!
            .GetAllTypes()
            .Single(t => t.Name == "<PrivateImplementationDetails>");

        _assemblyData = new()
        {
            AssemblyDefinition = assemblyDefinition,
            PrivateImplementationDetailType = type
        };
    }
}