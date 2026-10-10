using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.Logic.MethodLogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;

public sealed class SwitchArmsMethodsExtractor : SwitchArmsExtractor<MethodLogicContainer, MethodLogicContainer>
{
    public SwitchArmsMethodsExtractor(
        GameModuleData gameModuleData,
        MethodLogicContainer outerLogic)
        : base(
            gameModuleData,
            outerLogic,
            new MethodLogicExtractorFactory())
    {
    }

    protected override void Setup(
        GameModuleData gameModuleData,
        CilInstruction outerInitializeContextInstruction,
        CilInstruction outerSwitchInstruction,
        ICilLabel switchEndLabel)
    {
        OuterLogic.PatchContextIntoContainer(
            gameModuleData,
            outerInitializeContextInstruction.Offset,
            switchEndLabel.Offset);
        OuterBody.Instructions.CalculateOffsets();
    }

    protected override List<CilInstruction> GetOuterToInnerIl(
        MethodLogicContainer outer,
        MethodLogicContainer inner,
        ICilLabel switchEndLabel)
    {
        CilLocalVariable? contextLocal = outer.ContextLocal;
        List<CilInstruction> transferToMethodIl = MethodLogicContainer.GetTransferFromOuterMethodIl(
            inner,
            contextLocal is not null ? [contextLocal] : []);
        transferToMethodIl.Add(new(Br, switchEndLabel));
        return transferToMethodIl;
    }
}