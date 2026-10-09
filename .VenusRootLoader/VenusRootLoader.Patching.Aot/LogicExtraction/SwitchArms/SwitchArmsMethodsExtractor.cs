using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

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
        OuterLogic.PatchMethodContext(
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
        CilLocalVariable? contextLocal = outer.ContextInfo?.ContextLocal;
        List<CilInstruction> transferToMethodIl = MethodLogicContainer.GetTransferFromOuterMethodIl(
            inner,
            contextLocal is not null ? [contextLocal] : []);
        transferToMethodIl.Add(new(Br, switchEndLabel));
        return transferToMethodIl;
    }
}