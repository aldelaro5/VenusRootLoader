using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using System.Collections;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.StateMachineUtils;

/// <summary>
/// This class allows to extract the logic of an outer <see cref="StateMachine"/>'s <see cref="IEnumerator.MoveNext"/>
/// to an inner one's <see cref="IEnumerator.MoveNext"/>. It is done in 2 steps where the first one prepares a state machine
/// and collects other relevant information for the second step which processes all the instructions in the IL segment to
/// the inner state machine. It supports various control flow scheme such as yield break, goto case if the IL is in a switch
/// arm and a way to go back to state the beginning at state 0.
/// </summary>
internal sealed class MoveNextLogicExtractor
{
    /// <summary>
    /// The inner state machine created as part of the first step of the extraction.
    /// </summary>
    public StateMachine InnerStateMachine { get; }

    private readonly StateMachine _outerStateMachine;
    private readonly CilInstruction _innerFirstInstruction;
    private readonly CilInstruction _innerLastInstruction;
    private readonly CilMethodBody _innerMoveNextBody;
    private readonly FieldDefinition? _innerContextField;
    private readonly HashSet<string> _innerStateMachineFieldNames;
    private readonly CilInstruction _stateSwitchInstruction;
    private readonly List<ICilLabel> _stateSwitchLabels = [];
    private readonly Dictionary<int, CilLocalVariable> _localIndexMapping;
    private readonly Dictionary<FieldDefinition, FieldDefinition> _fieldsContextMapping;
    private readonly Dictionary<FieldDefinition, FieldDefinition> _fieldsNonContextMapping;
    private readonly int _innerInstructionIndexAfterStateMachineSetup;

    /// <summary>
    /// Creates an extractor using the information provided. This will perform the first step of the extraction process.
    /// The second step is done by calling <see cref="ExtractMoveNextIl"/> and it should be done once all the related state
    /// machines are created in the case of extracting all the arms of a switch.
    /// </summary>
    /// <param name="outerStateMachine">The outer state machine to extract logic from.</param>
    /// <param name="referenceImporter">The <see cref="ReferenceImporter"/> to use when creating the state machine.</param>
    /// <param name="stateMachineEnumeratorMethodName">The name the enumerator method of the inner state machine will have.</param>
    /// <param name="parameters">The parameters the enumerator method the inner state machine will have.</param>
    /// <param name="innerFirstInstruction">The IL offset of the starting point of the IL segment to extract from the <paramref name="outerStateMachine"/></param>
    /// <param name="innerLastInstruction">The IL offset of the ending point of the IL segment to extract from the <paramref name="outerStateMachine"/></param>
    /// <param name="fieldsContextMapping">A mapping to use to map <paramref name="outerStateMachine"/>'s fields to their
    /// context counterpart. This can be empty. For more information on this dictionary see <see cref="StateMachine.PatchStateMachineContextContext"/>.</param>
    public MoveNextLogicExtractor(
        StateMachine outerStateMachine,
        LocalNetStandardReferenceImporter referenceImporter,
        string stateMachineEnumeratorMethodName,
        List<NamedParameter> parameters,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction,
        Dictionary<FieldDefinition, FieldDefinition> fieldsContextMapping)
    {
        _outerStateMachine = outerStateMachine;
        InnerStateMachine = InnerStateMachineCreator.CreateAndAddInnerStateMachineType(
            outerStateMachine,
            referenceImporter,
            parameters,
            stateMachineEnumeratorMethodName);
        _innerFirstInstruction = innerFirstInstruction;
        _innerLastInstruction = innerLastInstruction;
        _fieldsContextMapping = fieldsContextMapping;

        ModuleDefinition module = outerStateMachine.StateMachineType.DeclaringModule!;
        _fieldsNonContextMapping = BuildFieldsMapping(
            module,
            outerStateMachine.StateMachineType,
            InnerStateMachine,
            fieldsContextMapping);
        _innerStateMachineFieldNames = InnerStateMachine.StateMachineType.Fields
            .Select(x => x.Name!.Value)
            .ToHashSet();
        CilLocalVariable localState = new(InnerStateMachine.StateField.Signature!.FieldType);
        CilLocalVariable? localThis = InnerStateMachine.ThisField is not null
            ? new(InnerStateMachine.ThisField.Signature!.FieldType)
            : null;
        _stateSwitchInstruction = CreateMoveNextBodyWithStateSwitchSetupInstructions(
            InnerStateMachine,
            localState,
            localThis);

        _localIndexMapping = new() { [0] = localState };
        if (localThis is not null)
            _localIndexMapping[1] = localThis;

        _innerMoveNextBody = InnerStateMachine.MoveNextMethod.CilMethodBody!;
        _innerInstructionIndexAfterStateMachineSetup = _innerMoveNextBody.Instructions.Count;
        _innerContextField = InnerStateMachine.StateMachineType.Fields.SingleOrDefault(x => x.Name == "context");
    }

    /// <summary>
    /// Performs the second step of the extraction process which will fill the inner state machine's <see cref="IEnumerator.MoveNext"/>
    /// using the logic of the outer state machine's <see cref="IEnumerator.MoveNext"/>. The logic will be processed such that
    /// the IL of the inner state machine will be adapted to work as a standalone method that's semantically equivalent.
    /// </summary>
    /// <param name="offsetsToStateMachines">A mappings of labels to other state machines. This will be used to map
    /// goto case statements if the state machine to extract is the arm of a switch. If this is not the case, this should
    /// be left empty.</param>
    /// <param name="yieldBreakLabel">A label that if encountered (or any instructions after it) will map to a yield break
    /// which means returning false.</param>
    /// <param name="instructionResetState">An instruction whose offset, if encountered, will map to the IL going back
    /// to the beginning of the method after setting the state to 0. If this flow isn't applicable, this should be null.</param>
    /// <param name="usedExceptionHandlerIndexes">The list of exception handler indexes that were copied to the inner state machine as a result of the extraction.</param>
    public void ExtractMoveNextIl(
        Dictionary<int, StateMachine> offsetsToStateMachines,
        ICilLabel yieldBreakLabel,
        CilInstruction? instructionResetState,
        out List<int> usedExceptionHandlerIndexes)
    {
        int nextStateNumber = 1;
        CilInstruction? instructionNeedsLabelFix = null;
        // We process from a clone because we need to mostly pick similar instructions, but we don't want the references
        // to clash with the original so this makes sure we won't be doing that.
        MethodDefinition outerMoveNextClone = CreateOuterMoveNextClone();
        usedExceptionHandlerIndexes = TransferExceptionHandlersFromClone(outerMoveNextClone);
        List<CilInstruction> innerIl = GetIlSegmentToProcessFromClone(outerMoveNextClone);

        foreach (CilInstruction instruction in innerIl)
        {
            if (instruction.IsLdloc() || instruction.OpCode == Ldloca || instruction.OpCode == Ldloca_S ||
                instruction.IsStloc())
            {
                // This will reindex the locals as they are used. This means the inner state machines will only have the
                // locals in needs instead of having all the locals of the other ones.
                ProcessLocalOperationInstruction(instruction);
            }
            else if ((instruction.OpCode == Ldfld || instruction.OpCode == Ldflda || instruction.OpCode == Stfld) &&
                     instruction.Operand is FieldDefinition fieldOperand)
            {
                // This processes 3 types of field operations: context fields, non context fields and switch state fields.
                // Depending on which of the 3 this is, it will map to a different scheme to process it so it works for
                // the inner state machine. Only fields actually used are added to the state machine.
                nextStateNumber = ProcessFieldOperationInstruction(fieldOperand, instruction, nextStateNumber);
            }
            else if (instruction.IsBranch() && instruction.Operand is ICilLabel instructionLabel)
            {
                // This processes 3 types of branching: yield break, goto case (to another switch arm) and a branch to
                // go back to the beginning of the logic.
                if (ProcessSpecialBranchOperationInstruction(
                        offsetsToStateMachines,
                        yieldBreakLabel,
                        instructionResetState,
                        instructionLabel,
                        instruction,
                        ref nextStateNumber,
                        ref instructionNeedsLabelFix))
                {
                    continue;
                }
            }

            // This is needed to commit the last state that we would have added.
            if (IsLastInstructionYieldReturn())
                _stateSwitchLabels.Add(instruction.CreateLabel());

            _innerMoveNextBody.Instructions.Add(instruction);

            if (instructionNeedsLabelFix is null)
                continue;

            // If we had processed a special branch earlier and this branch was conditional, the way we would make it work
            // is have the false segment be a branch instruction that skips over the instruction of the true segment.
            // Since the branch instruction needs to lead to the first instruction after the true segment, we need to fix
            // the label after that instruction was processed.
            instructionNeedsLabelFix.Operand = _innerMoveNextBody.Instructions[^1].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        _stateSwitchInstruction.Operand = _stateSwitchLabels;
        if (!IsLastInstructionYieldBreak())
            AddFinalYieldBreak();

        _innerMoveNextBody.Instructions.OptimizeMacros();
        _innerMoveNextBody.Instructions.CalculateOffsets();
    }

    private List<CilInstruction> GetIlSegmentToProcessFromClone(MethodDefinition outerMoveNextClone)
    {
        CilInstructionCollection outerMoveNextCloneIl = outerMoveNextClone.CilMethodBody!.Instructions;
        int start = outerMoveNextCloneIl.GetIndexByOffset(_innerFirstInstruction.Offset);
        int end = outerMoveNextCloneIl.GetIndexByOffset(_innerLastInstruction.Offset);
        return outerMoveNextCloneIl
            .Skip(start)
            .Take(end - start + 1)
            .ToList();
    }

    private List<int> TransferExceptionHandlersFromClone(MethodDefinition outerMoveNextClone)
    {
        IList<CilExceptionHandler> exceptionHandlers = outerMoveNextClone.CilMethodBody!.ExceptionHandlers;
        List<int> indexesExceptionHandlersToRemoveFromOriginal = [];
        for (int i = 0; i < exceptionHandlers.Count; i++)
        {
            CilExceptionHandler exceptionHandler = exceptionHandlers[i];
            if (!IsExceptionHandlerInRange(exceptionHandler))
                continue;
            InnerStateMachine.MoveNextMethod.CilMethodBody!.ExceptionHandlers.Add(exceptionHandler);
            indexesExceptionHandlersToRemoveFromOriginal.Add(i);
        }

        return indexesExceptionHandlersToRemoveFromOriginal;
    }

    private bool IsExceptionHandlerInRange(CilExceptionHandler exceptionHandler)
    {
        if (exceptionHandler.TryStart is not null
            && exceptionHandler.TryStart.Offset >= _innerFirstInstruction.Offset
            && exceptionHandler.TryEnd is not null
            && exceptionHandler.TryEnd.Offset <= _innerLastInstruction.Offset)
        {
            return true;
        }

        return exceptionHandler.HandlerStart is not null
               && exceptionHandler.HandlerStart.Offset >= _innerFirstInstruction.Offset
               && exceptionHandler.HandlerEnd is not null
               && exceptionHandler.HandlerEnd.Offset <= _innerLastInstruction.Offset;
    }

    private MethodDefinition CreateOuterMoveNextClone()
    {
        MemberCloner cloner = new(_outerStateMachine.StateMachineType.DeclaringModule!);
        cloner.Include(_outerStateMachine.MoveNextMethod);
        MemberCloneResult cloneResult = cloner.Clone();
        MethodDefinition outerMoveNextClone = cloneResult.GetClonedMember(_outerStateMachine.MoveNextMethod);
        return outerMoveNextClone;
    }

    private static Dictionary<FieldDefinition, FieldDefinition> BuildFieldsMapping(
        ModuleDefinition module,
        TypeDefinition typeToCloneFieldsFrom,
        StateMachine innerStateMachine,
        Dictionary<FieldDefinition, FieldDefinition> fieldsContextMapping)
    {
        MemberCloner cloner = new(module);
        foreach (FieldDefinition fieldDefinition in typeToCloneFieldsFrom.Fields)
            cloner.Include(fieldDefinition);

        MemberCloneResult clone = cloner.Clone();

        Dictionary<FieldDefinition, FieldDefinition> fieldsMapping = new();
        foreach (FieldDefinition originalField in clone.OriginalMembers.Cast<FieldDefinition>())
        {
            FieldDefinition clonedField = clone.GetClonedMember(originalField);
            // The special fields must always be in the mappings.
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

    private static CilInstruction CreateMoveNextBodyWithStateSwitchSetupInstructions(
        StateMachine innerStateMachine,
        CilLocalVariable localState,
        CilLocalVariable? localThis)
    {
        CilInstruction stateSwitchInstruction = new(Switch);
        innerStateMachine.MoveNextMethod.CilMethodBody = new()
        {
            InitializeLocals = true,
            LocalVariables = { localState },
            Instructions =
            {
                Ldarg_0,
                { Ldfld, innerStateMachine.StateField },
                { Stloc, localState }
            }
        };

        if (localThis is not null)
            innerStateMachine.MoveNextMethod.CilMethodBody.LocalVariables.Add(localThis);

        if (localThis is not null)
        {
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldarg_0);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldfld, innerStateMachine.ThisField!);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Stloc, localThis);
        }

        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldloc, localState);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(stateSwitchInstruction);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldc_I4_0);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ret);

        return stateSwitchInstruction;
    }

    private void ProcessLocalOperationInstruction(CilInstruction instruction)
    {
        CilLocalVariable local = instruction
            .GetLocalVariable(_outerStateMachine.MoveNextMethod.CilMethodBody!.LocalVariables);
        if (!_localIndexMapping.TryGetValue(local.Index, out CilLocalVariable? mappedLocal))
        {
            CilLocalVariable newLocal = new(local.VariableType);
            _innerMoveNextBody.LocalVariables.Add(newLocal);
            _localIndexMapping.Add(local.Index, newLocal);
            mappedLocal = newLocal;
        }

        instruction.Operand = mappedLocal;
    }

    private int ProcessFieldOperationInstruction(
        FieldDefinition fieldOperand,
        CilInstruction instruction,
        int nextStateNumber)
    {
        if (fieldOperand.DeclaringType == _outerStateMachine.StateMachineType)
        {
            FieldDefinition instructionField = (FieldDefinition)instruction.Operand!;
            if (_fieldsContextMapping.TryGetValue(instructionField, out FieldDefinition? fieldInContext))
                ProcessContextFieldOperationInstruction(instruction, fieldInContext);
            else
                ProcessNonContextFieldOperationInstruction(instruction, instructionField);
        }

        if (_innerMoveNextBody.Instructions.Count > _innerInstructionIndexAfterStateMachineSetup
            && instruction.OpCode == Stfld
            && instruction.Operand == InnerStateMachine.StateField
            && _innerMoveNextBody.Instructions[^1].GetLdcI4Constant() > -1)
        {
            _innerMoveNextBody.Instructions[^1].Operand = nextStateNumber;
            nextStateNumber++;
        }

        return nextStateNumber;
    }

    private void ProcessContextFieldOperationInstruction(CilInstruction instruction, FieldDefinition fieldInContext)
    {
        // Mapping a context field has a complication: the field belongs to the context, not to a field of the state machine.
        // Because of this, we need to insert an ldfld, but to figure out where, we do the assumption that the state mmachine
        // was placed on the stack using ldarg.0. We need to look for one that didn't had an ldfld right after so we can
        // insert our ldfld and have the field resolve correctly.
        int indexLastLdArg0 = -1;
        for (int j = _innerMoveNextBody.Instructions.Count - 1; j >= _innerInstructionIndexAfterStateMachineSetup; j--)
        {
            if (!_innerMoveNextBody.Instructions[j].IsLdarg() ||
                _innerMoveNextBody.Instructions[j].Operand is not Parameter { MethodSignatureIndex: 0 })
                continue;

            if (j != _innerMoveNextBody.Instructions.Count - 1 &&
                _innerMoveNextBody.Instructions[j + 1].OpCode == Ldfld)
            {
                continue;
            }

            indexLastLdArg0 = j;
            break;
        }

        _innerMoveNextBody.Instructions.Insert(indexLastLdArg0 + 1, Ldfld, _innerContextField!);
        instruction.Operand = fieldInContext;
    }

    private void ProcessNonContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        FieldDefinition remappedField = _fieldsNonContextMapping[instructionField];
        instruction.Operand = remappedField;
        if (remappedField.Name is null || _innerStateMachineFieldNames.Contains(remappedField.Name.Value))
            return;

        InnerStateMachine.StateMachineType.Fields.Add(remappedField);
        _innerStateMachineFieldNames.Add(remappedField.Name);
    }

    private bool ProcessSpecialBranchOperationInstruction(
        Dictionary<int, StateMachine> offsetsToStateMachines,
        ICilLabel yieldBreakLabel,
        CilInstruction? instructionResetState,
        ICilLabel label,
        CilInstruction instruction,
        ref int nextStateNumber,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        if (label.Offset >= yieldBreakLabel.Offset)
        {
            ProcessBranchInstructionAsYieldBreak(instruction, label, ref instructionNeedsLabelFix);
            return true;
        }

        if (offsetsToStateMachines.TryGetValue(label.Offset, out StateMachine? otherStateMachineInfo))
        {
            if (otherStateMachineInfo == InnerStateMachine)
            {
                ProcessBranchInstructionAsResetToState0(instruction, label, ref instructionNeedsLabelFix);
                return true;
            }

            ProcessBranchInstructionAsYieldReturnEnumerator(
                otherStateMachineInfo,
                nextStateNumber,
                instruction,
                label,
                ref instructionNeedsLabelFix);
            nextStateNumber++;

            return true;
        }

        if (instructionResetState is not null && instructionResetState.Offset == label.Offset)
        {
            ProcessBranchInstructionAsResetToState0(instruction, label, ref instructionNeedsLabelFix);
            return true;
        }

        return false;
    }

    private void ProcessBranchInstructionAsYieldBreak(
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        CilInstruction firstInstruction = new(Ldarg_0);

        PreProcessSpecialBranchOperationInstruction(
            instruction,
            label,
            firstInstruction,
            ref instructionNeedsLabelFix);

        _innerMoveNextBody.Instructions.Add(firstInstruction);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = _innerMoveNextBody.Instructions[^1].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        _innerMoveNextBody.Instructions.Add(Ldc_I4_M1);
        _innerMoveNextBody.Instructions.Add(Stfld, InnerStateMachine.StateField);
        _innerMoveNextBody.Instructions.Add(Ldc_I4_0);
        _innerMoveNextBody.Instructions.Add(Ret);
    }

    private void ProcessBranchInstructionAsYieldReturnEnumerator(
        StateMachine otherStateMachineInfo,
        int nextStateNumber,
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        List<CilInstruction> stateTransitionIl = InnerStateMachine.GetYieldReturnToOtherStateMachineInstructions(
            otherStateMachineInfo,
            _innerContextField is not null ? [_innerContextField] : [],
            nextStateNumber);

        PreProcessSpecialBranchOperationInstruction(
            instruction,
            label,
            stateTransitionIl[0],
            ref instructionNeedsLabelFix);

        _innerMoveNextBody.Instructions.AddRange(stateTransitionIl);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = stateTransitionIl[0].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        // The yield break after is needed because this acts like a goto case where the logic is performed, but the switch
        // is done after the destination arm is done.
        _innerMoveNextBody.Instructions.Add(Ldarg_0);
        _stateSwitchLabels.Add(_innerMoveNextBody.Instructions[^1].CreateLabel());
        _innerMoveNextBody.Instructions.Add(Ldc_I4_M1);
        _innerMoveNextBody.Instructions.Add(Stfld, InnerStateMachine.StateField);
        _innerMoveNextBody.Instructions.Add(Ldc_I4_0);
        _innerMoveNextBody.Instructions.Add(Ret);
    }

    private void ProcessBranchInstructionAsResetToState0(
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        CilInstruction state0FirstInstruction =
            _innerMoveNextBody.Instructions[_innerInstructionIndexAfterStateMachineSetup];
        CilInstruction resetIl = new(Br, state0FirstInstruction.CreateLabel());

        PreProcessSpecialBranchOperationInstruction(instruction, label, resetIl, ref instructionNeedsLabelFix);

        _innerMoveNextBody.Instructions.Add(resetIl);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = _innerMoveNextBody.Instructions[^1].CreateLabel();
            instructionNeedsLabelFix = null;
        }
    }

    private void PreProcessSpecialBranchOperationInstruction(
        CilInstruction instruction,
        ICilLabel label,
        CilInstruction firstInstructionAfterPreProcess,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        if (instruction.IsConditionalBranch())
        {
            if (IsLastInstructionYieldReturn())
                _stateSwitchLabels.Add(instruction.CreateLabel());

            _innerMoveNextBody.Instructions.Add(instruction);
            if (instructionNeedsLabelFix is not null)
            {
                instructionNeedsLabelFix.Operand = _innerMoveNextBody.Instructions[^1].CreateLabel();
                instructionNeedsLabelFix = null;
            }

            _innerMoveNextBody.Instructions[^1].Operand = new CilInstructionLabel(firstInstructionAfterPreProcess);
            _innerMoveNextBody.Instructions.Add(Br, label);
            instructionNeedsLabelFix = _innerMoveNextBody.Instructions[^1];
            return;
        }

        if (IsLastInstructionYieldReturn())
            _stateSwitchLabels.Add(firstInstructionAfterPreProcess.CreateLabel());
    }

    private bool IsLastInstructionYieldReturn()
    {
        return _innerMoveNextBody.Instructions.Count == _innerInstructionIndexAfterStateMachineSetup ||
               (_innerMoveNextBody.Instructions[^1].OpCode == Ret &&
                _innerMoveNextBody.Instructions[^2].GetLdcI4Constant() == 1);
    }

    private bool IsLastInstructionYieldBreak()
    {
        return _innerMoveNextBody.Instructions.Count == _innerInstructionIndexAfterStateMachineSetup ||
               (_innerMoveNextBody.Instructions[^1].OpCode == Ret &&
                _innerMoveNextBody.Instructions[^2].GetLdcI4Constant() == 0);
    }

    private void AddFinalYieldBreak()
    {
        _innerMoveNextBody.Instructions.Add(Ldarg_0);
        _innerMoveNextBody.Instructions.Add(Ldc_I4_M1);
        _innerMoveNextBody.Instructions.Add(Stfld, InnerStateMachine.StateField);
        _innerMoveNextBody.Instructions.Add(Ldc_I4_0);
        _innerMoveNextBody.Instructions.Add(Ret);
    }
}