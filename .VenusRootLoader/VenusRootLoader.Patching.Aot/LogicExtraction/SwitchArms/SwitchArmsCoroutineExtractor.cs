using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;

/// <summary>
/// Represents the top level class that can extract all the arms of a switch inside a coroutine to smaller coroutines for
/// each arm.
/// </summary>
internal sealed class SwitchArmsCoroutineExtractor : SwitchArmsExtractor<StateMachine, StateMachine>
{
    private readonly CilInstruction _stateSwitchInstruction;
    private readonly IList<ICilLabel> _stateSwitchLabels;
    private int _postSwitchStateNumber;
    private int _lastSwitchStateNumber;

    public SwitchArmsCoroutineExtractor(GameModuleData gameModuleData, StateMachine outerLogic)
        : base(gameModuleData, outerLogic, new MoveNextLogicExtractorFactory())
    {
        AsmResolverIlCursor ilCursor = new(OuterBody.Instructions);
        // The first switch is always the state switch.
        ilCursor.MatchNext(x => x.OpCode == Switch);
        _stateSwitchInstruction = ilCursor.Current();
        _stateSwitchLabels = (IList<ICilLabel>)_stateSwitchInstruction.Operand!;
    }

    protected override List<CilInstruction> GetOuterToInnerIl(
        StateMachine outer,
        StateMachine inner,
        ICilLabel switchEndLabel)
    {
        return outer.GetTransferToOtherStateMachineIl(
            inner,
            OuterLogic.ContextInfo?.ContextField is not null
                ? [OuterLogic.ContextInfo.ContextField]
                : [],
            _postSwitchStateNumber,
            switchEndLabel);
    }

    protected override void Setup(
        GameModuleData gameModuleData,
        CilInstruction outerInitializeContextInstruction,
        CilInstruction outerSwitchInstruction,
        ICilLabel switchEndLabel)
    {
        AsmResolverIlCursor ilCursor = new(OuterBody.Instructions);
        ilCursor.Index = OuterBody.Instructions.GetIndexByOffset(outerSwitchInstruction.Offset);
        int switchStateNumber = ilCursor.ObtainCurrentStateMachineState();
        // We reserve this state numbers for ourselves to process exiting the switch since we need to yield return. This
        // is better than to create a new state because it preserves the sequential nature of the states. so at runtime,
        // libraries that messes with state machines can still assume all states are in sequential order.
        _postSwitchStateNumber = switchStateNumber + 1;
        ilCursor.Index = OuterBody.Instructions.GetIndexByOffset(switchEndLabel.Offset);
        _lastSwitchStateNumber = ilCursor.ObtainCurrentStateMachineState();
        ilCursor.Index = OuterBody.Instructions.GetIndexByOffset(switchEndLabel.Offset);

        OuterLogic.PatchStateMachineContext(
            gameModuleData,
            outerInitializeContextInstruction.Offset,
            switchEndLabel.Offset);

        OuterBody.Instructions.CalculateOffsets();

        // Since we are creating a state for each of the switch's arm, we want to insert IL to set the state to -1 which
        // is what the compiler normally does. This isn't strictly necessary, but it's better to be safe just in case.
        ilCursor.Index = OuterBody.Instructions.GetIndexByOffset(switchEndLabel.Offset);
        CilInstruction instructionSwitchEnd = ilCursor.Current();
        OuterBody.Instructions.ReplaceRange(
            ilCursor.Index,
            ilCursor.Index,
            [
                new(Ldarg_0),
                new(Ldc_I4_M1),
                new(Stfld, OuterLogic.StateField),
                new(instructionSwitchEnd.OpCode, instructionSwitchEnd.Operand)
            ]);
        OuterBody.Instructions.CalculateOffsets();
    }

    protected override void PostExtraction(ICilLabel switchEndLabel)
    {
        // We reserve one state for ourselves to exit the switch, but after that one until the actual switch end state
        // the game used, we need to redirect all of them to the end of the switch so there's no chances of something
        // going in the middle of it. At runtime, no other switch state other than the one we assigned should be used,
        // this is just to be extra safe.
        for (int i = _postSwitchStateNumber; i <= _lastSwitchStateNumber; i++)
            _stateSwitchLabels[i] = switchEndLabel;

        _stateSwitchInstruction.Operand = _stateSwitchLabels;
    }
}