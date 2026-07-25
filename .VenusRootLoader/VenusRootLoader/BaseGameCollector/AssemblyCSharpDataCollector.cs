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

    /// <summary>
    /// Obtains all the events' method definitions of the game indexed by their underlying event id.
    /// </summary>
    /// <returns>A dictionary of method definitions indexed by their underlying event id.</returns>
    Dictionary<int, MethodDefinition> GetEventControlEvents();
}

/// <inheritdoc/>
internal sealed class AssemblyCSharpDataCollector : IAssemblyCSharpDataCollector
{
    private class AssemblyData
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        internal required AssemblyDefinition AssemblyDefinition { get; init; }
        internal required TypeDefinition PrivateImplementationDetailType { get; init; }
        internal required TypeDefinition EventControlType { get; init; }
    }

    private readonly GameExecutionContext _executionContext;
    private readonly IFileSystem _fileSystem;

    private AssemblyData _assemblyData;

    public AssemblyCSharpDataCollector(GameExecutionContext executionContext, IFileSystem fileSystem)
    {
        _executionContext = executionContext;
        _fileSystem = fileSystem;
        InitialiseAssemblyData();
    }

    public int[] ReadIntArrayFromPrivateImplementationDetailField(FieldInfo field)
    {
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

    public Dictionary<int, MethodDefinition> GetEventControlEvents()
    {
        return _assemblyData.EventControlType
            .Methods
            .Where(m => m.Name is not null && m.Name.Value.StartsWith("Event"))
            .ToDictionary(m => int.Parse(m.Name!.Value[5..]), m => m);
    }

    [MemberNotNull(nameof(_assemblyData))]
    private void InitialiseAssemblyData()
    {
        string assemblyPath = _fileSystem.Path.Combine(_executionContext.DataDir, "Managed", "Assembly-CSharp.dll");
        AssemblyDefinition assemblyDefinition = AssemblyDefinition.FromFile(assemblyPath);
        TypeDefinition privateImplementationDetailType = assemblyDefinition.ManifestModule!
            .GetAllTypes()
            .Single(t => t.Name == "<PrivateImplementationDetails>");
        TypeDefinition eventControlType = assemblyDefinition.ManifestModule
            .GetAllTypes()
            .Single(t => t.Name == nameof(EventControl));

        _assemblyData = new()
        {
            AssemblyDefinition = assemblyDefinition,
            PrivateImplementationDetailType = privateImplementationDetailType,
            EventControlType = eventControlType
        };
    }
}