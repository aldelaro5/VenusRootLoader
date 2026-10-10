using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.Logic;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

/// <summary>
/// Allows to create an extractor that can extract logic from an outer continer to an inner one.
/// </summary>
/// <typeparam name="TOuter">The type of the outer container.</typeparam>
/// <typeparam name="TInner">The type of the inner container.</typeparam>
public interface ILogicExtractorFactory<TOuter, TInner>
    where TOuter : LogicContainer
    where TInner : LogicContainer
{
    /// <summary>
    /// Creates an extractor that can extract logic from an outer container to an inner one.
    /// </summary>
    /// <param name="outerContainer">The outer container.</param>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="containerName">The name to give to the inner container.</param>
    /// <param name="innerFirstInstruction">The first instruction of the logic to extract from the outer container.</param>
    /// <param name="innerLastInstruction">The last instruction of the logic to extract from the outer container.</param>
    /// <returns>The logic extractor to extract the logic from <paramref name="innerFirstInstruction"/> to
    /// <paramref name="innerLastInstruction"/> that's inside <paramref name="outerContainer"/></returns>
    LogicExtractor<TOuter, TInner> Create(
        TOuter outerContainer,
        GameModuleData gameModuleData,
        string containerName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction);
}