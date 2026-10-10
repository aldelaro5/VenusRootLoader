using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Extractor.Factory;

/// <summary>
/// Allows to create a logic extractor that can extracts logic from an outer <see cref="StateMachine"/> to an inner
/// <see cref="StateMachine"/>.
/// </summary>
public sealed class MoveNextLogicExtractorFactory : ILogicExtractorFactory<StateMachine, StateMachine>
{
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