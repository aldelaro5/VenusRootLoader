using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

/// <summary>
/// Allows to create a logic extractor that can extracts logic from an outer state machine to an inner state machine.
/// </summary>
public sealed class MoveNextLogicExtractorFactory : ILogicExtractorFactory<StateMachine, StateMachine>
{
    /// <summary>
    /// Creates a logic extractor that can extract logic from an outer state machine to an inner state machine.
    /// </summary>
    /// <param name="outerContainer">The outer state machine.</param>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="containerName">The name to give the inner state machine's enumerator method.</param>
    /// <param name="innerFirstInstruction">The IL offset of the starting point of the IL segment to extract from the <paramref name="outerContainer"/></param>
    /// <param name="innerLastInstruction">The IL offset of the ending point of the IL segment to extract from the <paramref name="outerContainer"/></param>
    /// <returns>The logic extractor to extract logic from <paramref name="outerContainer"/> to an inner state machine.</returns>
    public LogicExtractor<StateMachine, StateMachine> Create(
        StateMachine outerContainer,
        GameModuleData gameModuleData,
        string containerName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
    {
        return new MoveNextLogicExtractor(
            outerContainer,
            gameModuleData,
            containerName,
            innerFirstInstruction,
            innerLastInstruction);
    }
}