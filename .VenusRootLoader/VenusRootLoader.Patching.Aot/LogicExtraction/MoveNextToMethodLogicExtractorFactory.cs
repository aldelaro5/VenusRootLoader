using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

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