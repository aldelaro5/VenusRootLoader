using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.Logic;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.Logic.MethodLogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;

public sealed class SwitchArmsCoroutineIntoMethodsExtractor : SwitchArmsExtractor<StateMachine, MethodLogicContainer>
{
    public SwitchArmsCoroutineIntoMethodsExtractor(
        GameModuleData gameModuleData,
        StateMachine outerLogic)
        : base(
            gameModuleData,
            outerLogic,
            new MoveNextToMethodLogicExtractorFactory())
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
        StateMachine outer,
        MethodLogicContainer inner,
        ICilLabel switchEndLabel)
    {
        List<FieldDefinition> fieldArguments = OuterLogic.ContextField is not null
            ? [OuterLogic.ContextField]
            : [];
        List<CilInstruction> transferToMethodIl = StateMachine.GetTransferToMethodIl(
            inner,
            fieldArguments);
        transferToMethodIl.Add(new(Br, switchEndLabel));
        return transferToMethodIl;
    }
}