using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.Logic;
using VenusRootLoader.Patching.Aot.Logic.ContainerFactory;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.Logic.MethodLogicContainer;

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