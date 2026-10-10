using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container;
using VenusRootLoader.Patching.Aot.LogicExtraction.Extractor.Factory;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.LogicExtraction.Container.MethodLogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Extractor.Switch;

/// <summary>
/// An extractor that can extract all the arms of a switch inside a <see cref="StateMachine"/> to smaller
/// <see cref="MethodLogicContainer"/>s for each arm. This only works if none of the arms performs a yield return.
/// </summary>
public sealed class SwitchArmsStateMachineIntoMethodsExtractor : SwitchArmsExtractor<StateMachine, MethodLogicContainer>
{
    public SwitchArmsStateMachineIntoMethodsExtractor(
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