using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;

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