using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container;
using VenusRootLoader.Patching.Aot.LogicExtraction.Container.Factory;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Extractor;

/// <summary>
/// An extractor that can extract logic from an outer <see cref="StateMachine"/> to an inner <see cref="StateMachine"/>.
/// </summary>
internal sealed class MoveNextLogicExtractor : LogicExtractor<StateMachine, StateMachine>
{
    private readonly FieldDefinition? _innerContextField;
    private readonly HashSet<string> _innerStateMachineFieldNames;
    private readonly CilInstruction _stateSwitchInstruction;
    private readonly List<ICilLabel> _stateSwitchLabels = [];
    private readonly Dictionary<FieldDefinition, FieldDefinition> _fieldsNonContextMapping;
    private readonly int _innerInstructionIndexAfterStateMachineSetup;
    private int _nextStateNumber = 1;

    public MoveNextLogicExtractor(
        StateMachine outerStateMachine,
        GameModuleData gameModuleData,
        string stateMachineEnumeratorMethodName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
        : base(
            outerStateMachine,
            new InnerStateMachineFactory(),
            gameModuleData,
            stateMachineEnumeratorMethodName,
            innerFirstInstruction,
            innerLastInstruction)
    {
        _fieldsNonContextMapping = BuildNonContextFieldsMapping(
            gameModuleData,
            outerStateMachine.StateMachineType,
            InnerLogic,
            outerStateMachine.Context?.ContextFieldsMapping ?? []);
        _innerStateMachineFieldNames = InnerLogic.StateMachineType.Fields
            .Select(x => x.Name!.Value)
            .ToHashSet();
        _stateSwitchInstruction = InnerBody.Instructions[^3];

        // We can assume these local indexes.
        LocalIndexMapping.Add(0, InnerBody.LocalVariables[0]);
        if (InnerLogic.ThisField is not null)
            LocalIndexMapping.Add(1, InnerBody.LocalVariables[1]);

        _innerInstructionIndexAfterStateMachineSetup = InnerBody.Instructions.Count;
        _innerContextField = InnerLogic.StateMachineType.Fields.SingleOrDefault(x => x.Name == "context");
    }

    private static Dictionary<FieldDefinition, FieldDefinition> BuildNonContextFieldsMapping(
        GameModuleData gameModuleData,
        TypeDefinition typeToCloneFieldsFrom,
        StateMachine innerStateMachine,
        Dictionary<object, FieldDefinition> fieldsContextMapping)
    {
        MemberCloner cloner = new(gameModuleData.Module);
        foreach (FieldDefinition fieldDefinition in typeToCloneFieldsFrom.Fields)
            cloner.Include(fieldDefinition);

        MemberCloneResult clone = cloner.Clone();

        Dictionary<FieldDefinition, FieldDefinition> fieldsMapping = new();
        foreach (FieldDefinition originalField in clone.OriginalMembers.Cast<FieldDefinition>())
        {
            // The special fields must always be in the mappings.
            FieldDefinition clonedField = clone.GetClonedMember(originalField);
            if (clonedField.Name == StateMachine.StateFieldName)
            {
                fieldsMapping.Add(originalField, innerStateMachine.StateField);
                continue;
            }

            if (clonedField.Name == StateMachine.CurrentFieldName)
            {
                fieldsMapping.Add(originalField, innerStateMachine.CurrentField);
                continue;
            }

            if (clonedField.Name == StateMachine.ThisFieldName)
            {
                fieldsMapping.Add(originalField, innerStateMachine.ThisField!);
                continue;
            }

            if (!fieldsContextMapping.ContainsKey(originalField))
                fieldsMapping.Add(originalField, clonedField);
        }

        return fieldsMapping;
    }

    protected override IList<CilInstruction> GetReturnToOuterIl(bool afterLogicTransfer)
    {
        List<CilInstruction> returnToOuterIl =
        [
            new(Ldarg_0),
            new(Ldc_I4_M1),
            new(Stfld, InnerLogic.StateField),
            new(Ldc_I4_0),
            new(Ret)
        ];

        // This matters because it means this yield break goes after a yield return which implies a new state just to yield break.
        if (afterLogicTransfer)
            _stateSwitchLabels.Add(returnToOuterIl[0].CreateLabel());
        return returnToOuterIl;
    }

    protected override ICilLabel GetRestartLabel()
    {
        CilInstruction state0FirstInstruction = InnerBody.Instructions[_innerInstructionIndexAfterStateMachineSetup];
        return state0FirstInstruction.CreateLabel();
    }

    protected override IList<CilInstruction> GetLogicTransferIl(
        StateMachine otherContainer,
        CilInstruction instructionAfter)
    {
        List<CilInstruction> stateTransitionIl = InnerLogic.GetTransferToOtherStateMachineIl(
            otherContainer,
            _innerContextField is not null ? [_innerContextField] : [],
            _nextStateNumber,
            instructionAfter.CreateLabel());
        _nextStateNumber++;
        return stateTransitionIl;
    }

    protected override void PostProcessFieldOperationInstruction(CilInstruction instruction)
    {
        if (InnerBody.Instructions.Count <= _innerInstructionIndexAfterStateMachineSetup
            || instruction.OpCode != Stfld
            || instruction.Operand != InnerLogic.StateField
            || InnerBody.Instructions[^1].GetLdcI4Constant() <= -1)
        {
            return;
        }

        // This effectively maps the states as we see them being used in the outer logic.
        InnerBody.Instructions[^1].Operand = _nextStateNumber;
        _nextStateNumber++;
    }

    protected override CilInstruction? ProcessArgumentOperationInstruction(
        CilInstruction instruction,
        Parameter instructionParameter)
    {
        return null;
    }

    protected override void ProcessFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        if (OuterLogic.Context is not null && OuterLogic.Context.ContextFieldsMapping.TryGetValue(
                instructionField,
                out FieldDefinition? fieldInContext))
            ProcessContextFieldOperationInstruction(instruction, fieldInContext);
        else
            ProcessNonContextFieldOperationInstruction(instruction, instructionField);
    }

    private void ProcessContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition fieldInContext)
    {
        if (instruction.OpCode == Ldfld || instruction.OpCode == Ldflda)
        {
            InnerBody.Instructions.Add(Ldfld, _innerContextField!);
            instruction.Operand = fieldInContext;
            return;
        }

        // Mapping a context field has a complication: the field belongs to the context, not to a field of the state machine.
        // Because of this, we need to insert an Ldfld, but to figure out where, we do the assumption that the state machine
        // was placed on the stack using Ldarg.0. We need to look for one that didn't have an Ldfld right after so we can
        // insert our Ldfld and have the field resolve correctly.
        int indexLastLdArg0 = -1;
        for (int j = InnerBody.Instructions.Count - 1; j >= _innerInstructionIndexAfterStateMachineSetup; j--)
        {
            if (!InnerBody.Instructions[j].IsLdarg() ||
                InnerBody.Instructions[j].Operand is not Parameter { MethodSignatureIndex: 0 })
                continue;

            if (j != InnerBody.Instructions.Count - 1 &&
                InnerBody.Instructions[j + 1].OpCode == Ldfld)
            {
                continue;
            }

            indexLastLdArg0 = j;
            break;
        }

        InnerBody.Instructions.Insert(indexLastLdArg0 + 1, Ldfld, _innerContextField!);
        instruction.Operand = fieldInContext;
    }

    private void ProcessNonContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        FieldDefinition remappedField = _fieldsNonContextMapping[instructionField];
        instruction.Operand = remappedField;
        // We check the field by name because the cloned one won't technically be the same, but the names would be.
        if (remappedField.Name is null || _innerStateMachineFieldNames.Contains(remappedField.Name.Value))
            return;

        InnerLogic.StateMachineType.Fields.Add(remappedField);
        _innerStateMachineFieldNames.Add(remappedField.Name);
    }

    protected override void BeforeAddingInstruction(CilInstruction instruction)
    {
        // This is needed to commit the last state that we would have added.
        if (IsLastInstructionYieldReturn())
            _stateSwitchLabels.Add(instruction.CreateLabel());
    }

    protected override void PostProcessExtraction()
    {
        _stateSwitchInstruction.Operand = _stateSwitchLabels;
        if (!IsLastInstructionYieldBreak())
            AddFinalYieldBreak();
    }

    private bool IsLastInstructionYieldReturn()
    {
        return InnerBody.Instructions.Count == _innerInstructionIndexAfterStateMachineSetup ||
               (InnerBody.Instructions[^1].OpCode == Ret &&
                InnerBody.Instructions[^2].GetLdcI4Constant() == 1);
    }

    private bool IsLastInstructionYieldBreak()
    {
        return InnerBody.Instructions.Count == _innerInstructionIndexAfterStateMachineSetup ||
               (InnerBody.Instructions[^1].OpCode == Ret &&
                InnerBody.Instructions[^2].GetLdcI4Constant() == 0);
    }

    private void AddFinalYieldBreak()
    {
        InnerBody.Instructions.Add(Ldarg_0);
        InnerBody.Instructions.Add(Ldc_I4_M1);
        InnerBody.Instructions.Add(Stfld, InnerLogic.StateField);
        InnerBody.Instructions.Add(Ldc_I4_0);
        InnerBody.Instructions.Add(Ret);
    }
}