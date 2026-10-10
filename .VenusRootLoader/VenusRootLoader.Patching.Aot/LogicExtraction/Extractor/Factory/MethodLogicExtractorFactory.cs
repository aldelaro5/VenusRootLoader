using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container.Factory;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.LogicExtraction.Container.MethodLogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Extractor.Factory;

/// <summary>
/// Allows to create a logic extractor that can extracts logic from an outer <see cref="MethodLogicContainer"/> to an
/// inner <see cref="MethodLogicContainer"/>.
/// </summary>
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