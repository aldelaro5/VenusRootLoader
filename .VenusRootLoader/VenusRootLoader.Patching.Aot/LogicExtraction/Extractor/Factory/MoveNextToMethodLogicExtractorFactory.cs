using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container.Factory;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.LogicExtraction.Container.MethodLogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Extractor.Factory;

/// <summary>
/// Allows to create a logic extractor that can extracts logic from an outer <see cref="StateMachine"/> to an
/// inner <see cref="MethodLogicContainer"/>.
/// </summary>
public sealed class MoveNextToMethodLogicExtractorFactory : ILogicExtractorFactory<StateMachine, MethodLogicContainer>
{
    public LogicExtractor<StateMachine, MethodLogicContainer> Create(
        StateMachine outerContainer,
        GameModuleData gameModuleData,
        string containerName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
    {
        return new MoveNextToMethodLogicExtractor(
            outerContainer,
            new InnerMethodFromCoroutineFactory(),
            gameModuleData,
            containerName,
            innerFirstInstruction,
            innerLastInstruction);
    }
}