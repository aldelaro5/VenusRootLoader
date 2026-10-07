using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot;

public abstract class LogicExtractor<TOuter, TInner>
    where TOuter : ILogicContainer
    where TInner : ILogicContainer
{
    /// <summary>
    /// The inner state machine created as part of the first step of the extraction.
    /// </summary>
    public TInner InnerLogic { get; }

    protected readonly CilMethodBody InnerBody;
    protected readonly Dictionary<int, CilLocalVariable> LocalIndexMapping = [];

    private readonly TOuter _outerLogic;
    private readonly CilInstruction _innerFirstInstruction;
    private readonly CilInstruction _innerLastInstruction;
    private readonly Dictionary<FieldDefinition, FieldDefinition> _fieldsContextMapping;

    protected abstract void ProcessContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition fieldInContext);

    protected abstract IList<CilInstruction> GetReturnToOuterIl(bool afterLogicTransfer);
    protected abstract ICilLabel GetRestartLabel();

    protected abstract IList<CilInstruction> GetLogicTransferIl(
        TInner otherContainer,
        CilInstruction instructionAfter);

    protected virtual void PostProcessFieldOperationInstruction(CilInstruction instruction)
    {
        return;
    }

    protected virtual void ProcessNonContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        return;
    }

    protected virtual void PostProcessExtraction()
    {
        return;
    }

    protected virtual void BeforeAddingInstruction(CilInstruction instruction)
    {
        return;
    }

    protected LogicExtractor(
        TOuter outerLogic,
        IInnerLogicContainerFactory<TOuter, TInner> innerLogicContainerFactory,
        GameModuleData gameModuleData,
        string methodName,
        List<NamedParameter> methodParameters,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction,
        Dictionary<FieldDefinition, FieldDefinition> fieldsContextMapping)
    {
        _outerLogic = outerLogic;
        _innerFirstInstruction = innerFirstInstruction;
        _innerLastInstruction = innerLastInstruction;
        _fieldsContextMapping = fieldsContextMapping;
        InnerLogic = innerLogicContainerFactory.Create(
            outerLogic,
            gameModuleData,
            methodParameters,
            methodName);
        InnerBody = InnerLogic.ReceivingMethod.CilMethodBody!;
    }

    public void ExtractIl(
        Dictionary<int, TInner> offsetsToStateMachines,
        ICilLabel yieldBreakLabel,
        CilInstruction? instructionResetState,
        out List<int> usedExceptionHandlerIndexes)
    {
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
                ProcessFieldOperationInstruction(fieldOperand, instruction);
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
                        ref instructionNeedsLabelFix))
                {
                    continue;
                }
            }

            // This is needed to commit the last state that we would have added.
            BeforeAddingInstruction(instruction);

            InnerBody.Instructions.Add(instruction);

            if (instructionNeedsLabelFix is null)
                continue;

            // If we had processed a special branch earlier and this branch was conditional, the way we would make it work
            // is have the false segment be a branch instruction that skips over the instruction of the true segment.
            // Since the branch instruction needs to lead to the first instruction after the true segment, we need to fix
            // the label after that instruction was processed.
            instructionNeedsLabelFix.Operand = InnerBody.Instructions[^1].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        PostProcessExtraction();

        InnerBody.Instructions.OptimizeMacros();
        InnerBody.Instructions.CalculateOffsets();
    }

    private MethodDefinition CreateOuterMoveNextClone()
    {
        MemberCloner cloner = new(_outerLogic.ReceivingMethod.DeclaringModule!);
        cloner.Include(_outerLogic.ReceivingMethod);
        MemberCloneResult cloneResult = cloner.Clone();
        MethodDefinition outerMoveNextClone = cloneResult.GetClonedMember(_outerLogic.ReceivingMethod);
        return outerMoveNextClone;
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
            InnerBody.ExceptionHandlers.Add(exceptionHandler);
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

    private void ProcessLocalOperationInstruction(CilInstruction instruction)
    {
        CilLocalVariable local = instruction
            .GetLocalVariable(_outerLogic.ReceivingMethod.CilMethodBody!.LocalVariables);
        if (!LocalIndexMapping.TryGetValue(local.Index, out CilLocalVariable? mappedLocal))
        {
            CilLocalVariable newLocal = new(local.VariableType);
            InnerBody.LocalVariables.Add(newLocal);
            InnerBody.InitializeLocals = true;
            LocalIndexMapping.Add(local.Index, newLocal);
            mappedLocal = newLocal;
        }

        instruction.Operand = mappedLocal;
    }

    private void ProcessFieldOperationInstruction(FieldDefinition fieldOperand, CilInstruction instruction)
    {
        if (fieldOperand.DeclaringType == _outerLogic.ReceivingMethod.DeclaringType)
        {
            FieldDefinition instructionField = (FieldDefinition)instruction.Operand!;
            if (_fieldsContextMapping.TryGetValue(instructionField, out FieldDefinition? fieldInContext))
                ProcessContextFieldOperationInstruction(instruction, fieldInContext);
            else
                ProcessNonContextFieldOperationInstruction(instruction, instructionField);
        }

        PostProcessFieldOperationInstruction(instruction);
    }

    private bool ProcessSpecialBranchOperationInstruction(
        Dictionary<int, TInner> offsetsToStateMachines,
        ICilLabel yieldBreakLabel,
        CilInstruction? instructionResetState,
        ICilLabel label,
        CilInstruction instruction,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        if (label.Offset >= yieldBreakLabel.Offset)
        {
            ProcessBranchInstructionAsYieldBreak(instruction, label, ref instructionNeedsLabelFix);
            return true;
        }

        if (offsetsToStateMachines.TryGetValue(label.Offset, out TInner? otherStateMachineInfo))
        {
            if (otherStateMachineInfo.Equals(InnerLogic))
            {
                ProcessBranchInstructionAsResetToState0(instruction, label, ref instructionNeedsLabelFix);
                return true;
            }

            ProcessBranchInstructionAsYieldReturnEnumerator(
                otherStateMachineInfo,
                instruction,
                label,
                ref instructionNeedsLabelFix);

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
        IList<CilInstruction> returnToOuterIl = GetReturnToOuterIl(false);
        CilInstruction firstInstruction = returnToOuterIl[0];

        PreProcessSpecialBranchOperationInstruction(
            instruction,
            label,
            firstInstruction,
            ref instructionNeedsLabelFix);

        BeforeAddingInstruction(firstInstruction);
        InnerBody.Instructions.Add(firstInstruction);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = InnerBody.Instructions[^1].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        InnerBody.Instructions.AddRange(returnToOuterIl.Skip(1));
    }

    private void ProcessBranchInstructionAsYieldReturnEnumerator(
        TInner otherStateMachineInfo,
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        IList<CilInstruction> returnToOuterIl = GetReturnToOuterIl(true);
        IList<CilInstruction> transferIl = GetLogicTransferIl(otherStateMachineInfo, returnToOuterIl[0]);

        PreProcessSpecialBranchOperationInstruction(
            instruction,
            label,
            transferIl[0],
            ref instructionNeedsLabelFix);

        BeforeAddingInstruction(transferIl[0]);
        InnerBody.Instructions.AddRange(transferIl);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = transferIl[0].CreateLabel();
            instructionNeedsLabelFix = null;
        }

        // The yield break after is needed because this acts like a goto case where the logic is performed, but the switch
        // is done after the destination arm is done.
        InnerBody.Instructions.AddRange(returnToOuterIl);
    }

    private void ProcessBranchInstructionAsResetToState0(
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        CilInstruction resetIl = new(Br, GetRestartLabel());

        PreProcessSpecialBranchOperationInstruction(instruction, label, resetIl, ref instructionNeedsLabelFix);

        BeforeAddingInstruction(resetIl);
        InnerBody.Instructions.Add(resetIl);
        if (neededFixing && instructionNeedsLabelFix is not null)
        {
            instructionNeedsLabelFix.Operand = InnerBody.Instructions[^1].CreateLabel();
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
            BeforeAddingInstruction(instruction);

            InnerBody.Instructions.Add(instruction);
            if (instructionNeedsLabelFix is not null)
            {
                instructionNeedsLabelFix.Operand = InnerBody.Instructions[^1].CreateLabel();
                instructionNeedsLabelFix = null;
            }

            InnerBody.Instructions[^1].Operand = new CilInstructionLabel(firstInstructionAfterPreProcess);
            InnerBody.Instructions.Add(Br, label);
            instructionNeedsLabelFix = InnerBody.Instructions[^1];
            return;
        }
    }
}