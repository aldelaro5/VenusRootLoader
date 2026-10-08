using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

/// <summary>
/// A logic extractor that extracts from an outer container to an inner container.
/// </summary>
/// <typeparam name="TOuter">The type of the outer container.</typeparam>
/// <typeparam name="TInner">The type of the inner container.</typeparam>
public abstract class LogicExtractor<TOuter, TInner>
    where TOuter : ILogicContainer
    where TInner : ILogicContainer
{
    /// <summary>
    /// The outer container.
    /// </summary>
    protected readonly TOuter OuterLogic;

    /// <summary>
    /// The inner container created as part of the first step of the extraction.
    /// </summary>
    public TInner InnerLogic { get; }

    /// <summary>
    /// A helper to access <see cref="InnerLogic"/>'s <see cref="CilMethodBody"/>.
    /// </summary>
    protected readonly CilMethodBody InnerBody;

    /// <summary>
    /// The mappings from their index in the outer container to the newly created ones in the inner container.
    /// </summary>
    protected readonly Dictionary<int, CilLocalVariable> LocalIndexMapping = [];

    private readonly CilInstruction _innerFirstInstruction;
    private readonly CilInstruction _innerLastInstruction;

    /// <summary>
    /// Process an instruction that involves a field.
    /// </summary>
    /// <param name="instruction">The instruction involved.</param>
    /// <param name="fieldInContext">The field involved in the instruction.</param>
    protected abstract void ProcessFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition fieldInContext);

    /// <summary>
    /// Obtains the IL needed to transfer control flow back to the outer container.
    /// </summary>
    /// <param name="afterLogicTransfer">Tells if this IL will be placed immediately after a logic transfer to another container.</param>
    /// <returns>The IL that transfer control back to the outer container.</returns>
    protected abstract IList<CilInstruction> GetReturnToOuterIl(bool afterLogicTransfer);

    /// <summary>
    /// Obtains a label that marks the start of the container so the IL can branch to it to restart the logic.
    /// </summary>
    /// <returns>The label that marks the start of the container.</returns>
    protected abstract ICilLabel GetRestartLabel();

    /// <summary>
    /// Obtains the IL that transfer control flow to another container.
    /// </summary>
    /// <param name="otherContainer">The container to transfer control flow into.</param>
    /// <param name="instructionAfter">The instruction that will be placed after the IL.</param>
    /// <returns>The IL that transfer control flow to <paramref name="otherContainer"/>.</returns>
    protected abstract IList<CilInstruction> GetLogicTransferIl(
        TInner otherContainer,
        CilInstruction instructionAfter);

    /// <summary>
    /// Performs tasks before adding a regular instruction.
    /// </summary>
    /// <param name="instruction">The instruction that will be added.</param>
    protected virtual void BeforeAddingInstruction(CilInstruction instruction)
    {
        return;
    }

    /// <summary>
    /// Performs tasks after processing an instruction involving a field.
    /// </summary>
    /// <param name="instruction">The instruction that was just processed.</param>
    protected virtual void PostProcessFieldOperationInstruction(CilInstruction instruction)
    {
        return;
    }

    /// <summary>
    /// Performs tasks before the end of the extractions.
    /// </summary>
    protected virtual void PostProcessExtraction()
    {
        return;
    }

    /// <summary>
    /// Creates a logic extractor to extract logic from an outer container to an inner container.
    /// </summary>
    /// <param name="outerLogic">The outer container.</param>
    /// <param name="innerLogicContainerFactory">A factory to create the inner container.</param>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="methodName">The name to assign the inner container.</param>
    /// <param name="innerFirstInstruction">The first instruction in the outer container to extract logic from.</param>
    /// <param name="innerLastInstruction">The last instruction in the outer container to extract logic from.</param>
    protected LogicExtractor(
        TOuter outerLogic,
        IInnerLogicContainerFactory<TOuter, TInner> innerLogicContainerFactory,
        GameModuleData gameModuleData,
        string methodName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
    {
        OuterLogic = outerLogic;
        _innerFirstInstruction = innerFirstInstruction;
        _innerLastInstruction = innerLastInstruction;

        NamedParameter? contextParameter = OuterLogic.GetContextParameter();
        InnerLogic = innerLogicContainerFactory.Create(
            outerLogic,
            gameModuleData,
            contextParameter is not null ? [contextParameter] : [],
            methodName);
        InnerBody = InnerLogic.ReceivingMethod.CilMethodBody!;
    }

    /// <summary>
    /// Extracts the IL into the inner container.
    /// </summary>
    /// <param name="offsetsToInnerContainers">A mapping from offsets to other contains used for goto flow mapping.</param>
    /// <param name="returnToOuterLabel">A label that marks a return to the outer container.</param>
    /// <param name="instructionRestart">A label that marks a restart of the logic.</param>
    /// <param name="usedExceptionHandlerIndexes">A list of exception handler indexes that were used from the outer container.</param>
    public void ExtractIl(
        Dictionary<int, TInner> offsetsToInnerContainers,
        ICilLabel returnToOuterLabel,
        CilInstruction? instructionRestart,
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
                // This will reindex the locals as they are used. This means the inner container will only have the
                // locals in needs instead of having all the locals of the other ones.
                ProcessLocalOperationInstruction(instruction);
            }
            else if ((instruction.OpCode == Ldfld || instruction.OpCode == Ldflda || instruction.OpCode == Stfld) &&
                     instruction.Operand is FieldDefinition fieldOperand)
            {
                ProcessFieldOperationInstruction(fieldOperand, instruction);
            }
            else if (instruction.IsBranch() && instruction.Operand is ICilLabel instructionLabel)
            {
                // This processes 3 types of branching: return, goto (to another container) and a branch to go back to
                // the beginning of the logic.
                if (ProcessSpecialBranchOperationInstruction(
                        offsetsToInnerContainers,
                        returnToOuterLabel,
                        instructionRestart,
                        instructionLabel,
                        instruction,
                        ref instructionNeedsLabelFix))
                {
                    continue;
                }
            }

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
        MemberCloner cloner = new(OuterLogic.ReceivingMethod.DeclaringModule!);
        cloner.Include(OuterLogic.ReceivingMethod);
        MemberCloneResult cloneResult = cloner.Clone();
        MethodDefinition outerMoveNextClone = cloneResult.GetClonedMember(OuterLogic.ReceivingMethod);
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
            .GetLocalVariable(OuterLogic.ReceivingMethod.CilMethodBody!.LocalVariables);
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
        if (fieldOperand.DeclaringType == OuterLogic.ReceivingMethod.DeclaringType)
        {
            FieldDefinition instructionField = (FieldDefinition)instruction.Operand!;
            ProcessFieldOperationInstruction(instruction, instructionField);
        }

        PostProcessFieldOperationInstruction(instruction);
    }

    private bool ProcessSpecialBranchOperationInstruction(
        Dictionary<int, TInner> offsetsToInnerContainers,
        ICilLabel returnToOuterLabel,
        CilInstruction? instructionRestart,
        ICilLabel label,
        CilInstruction instruction,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        if (label.Offset >= returnToOuterLabel.Offset)
        {
            ProcessBranchInstructionAsReturnToOuter(instruction, label, ref instructionNeedsLabelFix);
            return true;
        }

        if (offsetsToInnerContainers.TryGetValue(label.Offset, out TInner? otherInnerContainer))
        {
            if (otherInnerContainer.Equals(InnerLogic))
            {
                ProcessBranchInstructionAsRestart(instruction, label, ref instructionNeedsLabelFix);
                return true;
            }

            ProcessBranchInstructionAsTransferToOtherContainer(
                otherInnerContainer,
                instruction,
                label,
                ref instructionNeedsLabelFix);

            return true;
        }

        if (instructionRestart is not null && instructionRestart.Offset == label.Offset)
        {
            ProcessBranchInstructionAsRestart(instruction, label, ref instructionNeedsLabelFix);
            return true;
        }

        return false;
    }

    private void ProcessBranchInstructionAsReturnToOuter(
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

    private void ProcessBranchInstructionAsTransferToOtherContainer(
        TInner otherInnerContainer,
        CilInstruction instruction,
        ICilLabel label,
        ref CilInstruction? instructionNeedsLabelFix)
    {
        bool neededFixing = instructionNeedsLabelFix is not null;
        IList<CilInstruction> returnToOuterIl = GetReturnToOuterIl(true);
        IList<CilInstruction> transferIl = GetLogicTransferIl(otherInnerContainer, returnToOuterIl[0]);

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

        // The return after is needed because this acts like a goto case where the logic is performed, but the switch
        // is done after the destination arm is done.
        InnerBody.Instructions.AddRange(returnToOuterIl);
    }

    private void ProcessBranchInstructionAsRestart(
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