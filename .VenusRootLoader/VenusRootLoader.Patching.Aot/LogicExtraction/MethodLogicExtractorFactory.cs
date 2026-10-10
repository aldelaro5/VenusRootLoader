using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.Logic;
using VenusRootLoader.Patching.Aot.Logic.ContainerFactory;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.Logic.MethodLogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

public sealed class MethodLogicExtractorFactory : ILogicExtractorFactory<MethodLogicContainer, MethodLogicContainer>
{
    public LogicExtractor<MethodLogicContainer, MethodLogicContainer> Create(
        MethodLogicContainer outerContainer,
        GameModuleData gameModuleData,
        string containerName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
    {
        return new MethodLogicExtractor(
            outerContainer,
            new InnerMethodFactory(),
            gameModuleData,
            containerName,
            innerFirstInstruction,
            innerLastInstruction);
    }
}