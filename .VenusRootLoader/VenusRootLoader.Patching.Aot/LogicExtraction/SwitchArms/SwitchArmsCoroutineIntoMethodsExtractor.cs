using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;

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
        OuterLogic.PatchStateMachineContext(
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
        List<FieldDefinition> fieldArguments = OuterLogic.ContextInfo?.ContextField is not null
            ? [OuterLogic.ContextInfo.ContextField]
            : [];
        return StateMachine.GetTransferToMethodIl(inner, fieldArguments, switchEndLabel);
    }
}