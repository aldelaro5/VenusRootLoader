using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System.Collections;
using VenusRootLoader.Patching.Aot.StateMachineUtils;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// This is an abstraction of what constitutes the code segment forming the arm of a switch. It's possible multiple switch
/// cases leads to the same arm so this abstraction encodes this by containing offsets to delimit the arm segment and the
/// labels that leads to it from the switch.
/// </summary>
internal sealed class SwitchArn
{
    /// <summary>
    /// The labels in the switch that leads to this arm.
    /// </summary>
    public required List<IndexedSwitchLabel> Labels { get; init; }

    /// <summary>
    /// The first instruction where this arm is located in the method.
    /// </summary>
    public required CilInstruction StartInstruction { get; init; }

    /// <summary>
    /// The last instruction where this arm is located in the method.
    /// </summary>
    public required CilInstruction EndInstruction { get; init; }
}

/// <summary>
/// An association between a <see cref="ICilLabel"/> and its index position within the switch it is contained in.
/// </summary>
internal sealed class IndexedSwitchLabel
{
    /// <summary>
    /// The label associated with a switch.
    /// </summary>
    public required ICilLabel Label { get; init; }

    /// <summary>
    /// The index of the label as it appears in a switch.
    /// </summary>
    public required int Index { get; init; }
}

/// <summary>
/// Represents the top level class that can extract all the arms of a switch inside a coroutine to smaller coroutines for
/// each arm.
/// </summary>
internal sealed class SwitchArmsCoroutineExtractor
{
    /// <summary>
    /// Extract all the switch arms of a <see cref="StateMachine"/> to their own inner state machine with support for a
    /// context parameter and various control flow mappings.
    /// </summary>
    /// <param name="gameModuleData">The <see cref="ReferenceImporter"/> to use when creating state machines or other
    /// constructs.</param>
    /// <param name="outerStateMachine">The <see cref="StateMachine"/> involved in the extraction process.</param>
    /// <param name="innerStateMachinesMethodPrefix">The prefix of all names to give to the inner state machines enumerator methods.</param>
    /// <param name="switchContextInfo">The information needed to create a context. If no context should be involved, this
    /// should be null.</param>
    /// <param name="outerSwitchInstruction">The switch instruction involved in the extraction of the
    /// <paramref name="outerStateMachine"/>'s <see cref="IEnumerator.MoveNext"/>.</param>
    /// <param name="outerInitializeContextInstruction">The instruction where the context initialization code will be
    /// inserted.</param>
    /// <param name="outerResetStateMachineToZeroInstruction">The instruction marking a reset of an inner state machine
    /// to its state 0 (such as a continue statement done in the switch arm as the entire switch is in a loop).</param>
    /// <param name="stateMachinePostProcessor">A callback to invoke after each inner state machines has been fully generated
    /// to perform special post-processing patches.</param>
    public static void ExtractSwitchArmsToStateMachines(
        GameModuleData gameModuleData,
        StateMachine outerStateMachine,
        string innerStateMachinesMethodPrefix,
        StateMachineContextInfo? switchContextInfo,
        CilInstruction outerSwitchInstruction,
        CilInstruction outerInitializeContextInstruction,
        CilInstruction? outerResetStateMachineToZeroInstruction,
        Action<int, StateMachine, GameModuleData>? stateMachinePostProcessor)
    {
        CilInstructionCollection moveNextIl = outerStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        moveNextIl.ExpandMacros();
        moveNextIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(moveNextIl);

        // The first switch is always the state switch.
        ilCursor.MatchNext(x => x.OpCode == Switch);
        CilInstruction stateSwitchInstruction = ilCursor.Current();
        IList<ICilLabel> stateSwitchLabels = (IList<ICilLabel>)stateSwitchInstruction.Operand!;

        ilCursor.Index = moveNextIl.GetIndexByOffset(outerSwitchInstruction.Offset);
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.Index++;
        ICilLabel switchEndLabel = (ICilLabel)ilCursor.Current().Operand!;
        List<SwitchArn> switchArms = OrganizeSwitchLabelsIntoArms(switchArmLabels, switchEndLabel, moveNextIl);

        int switchStateNumber = ilCursor.ObtainCurrentStateMachineState();
        // We reserve this state numbers for ourselves to process exiting the switch since we need to yield return. This
        // is better than to create a new state because it preserves the sequential nature of the states. so at runtime,
        // libraries that messes with state machines can still assume all states are in sequential order.
        int postSwitchStateNumber = switchStateNumber + 1;
        ilCursor.Index = moveNextIl.GetIndexByOffset(switchEndLabel.Offset);
        int lastSwitchStateNumber = ilCursor.ObtainCurrentStateMachineState();

        Dictionary<FieldDefinition, FieldDefinition> fieldsContextMapping = new();
        FieldDefinition? contextField = switchContextInfo is not null
            ? outerStateMachine.PatchStateMachineContextContext(
                gameModuleData,
                switchContextInfo,
                outerInitializeContextInstruction.Offset,
                switchEndLabel.Offset,
                fieldsContextMapping)
            : null;

        // We'll always use the same parameter type and name so might as well keep reusing it.
        NamedParameter? contextParameter = contextField is not null
            ? new()
            {
                Name = "context",
                TypeSignature = contextField.Signature!.FieldType
            }
            : null;

        Dictionary<CilInstruction, StateMachine> firstInstructionToStateMachine = new();
        Dictionary<int, MoveNextLogicExtractor> labelIndexesToExtractors = new();
        // The first pass creates all the state machines. This allows goto case flow to map even if we encounter an arm
        // we haven't processed yet.
        foreach (SwitchArn arm in switchArms)
        foreach (IndexedSwitchLabel indexedSwitchLabel in arm.Labels)
        {
            MoveNextLogicExtractor extractor = new(
                outerStateMachine,
                gameModuleData,
                $"{innerStateMachinesMethodPrefix}{indexedSwitchLabel.Index}",
                contextParameter is not null ? [contextParameter] : [],
                arm.StartInstruction,
                arm.EndInstruction,
                fieldsContextMapping);

            labelIndexesToExtractors[indexedSwitchLabel.Index] = extractor;
            firstInstructionToStateMachine[((CilInstructionLabel)indexedSwitchLabel.Label).Instruction!] =
                extractor.InnerStateMachine;
        }

        foreach (SwitchArn arm in switchArms)
        {
            moveNextIl.CalculateOffsets();
            Dictionary<int, StateMachine> offsetsToStateMachine =
                firstInstructionToStateMachine.ToDictionary(x => x.Key.Offset, x => x.Value);

            List<StateMachine> stateMachines = new();
            foreach (IndexedSwitchLabel indexedSwitchArmLabel in arm.Labels)
            {
                StateMachine innerStateMachine =
                    labelIndexesToExtractors[indexedSwitchArmLabel.Index].InnerStateMachine;
                stateMachines.Add(innerStateMachine);

                labelIndexesToExtractors[indexedSwitchArmLabel.Index].ExtractMoveNextIl(
                    offsetsToStateMachine,
                    switchEndLabel,
                    outerResetStateMachineToZeroInstruction,
                    out List<int> usedExceptionHandlerIndexes);

                foreach (int exceptionHandlerIndex in usedExceptionHandlerIndexes.OrderDescending())
                    outerStateMachine.MoveNextMethod.CilMethodBody!.ExceptionHandlers.RemoveAt(exceptionHandlerIndex);

                stateMachinePostProcessor?.Invoke(indexedSwitchArmLabel.Index, innerStateMachine, gameModuleData);
            }

            int start = moveNextIl.GetIndexByOffset(arm.StartInstruction.Offset);
            int end = moveNextIl.GetIndexByOffset(arm.EndInstruction.Offset);
            // The reason we yield return multiple times in a row is to accomodate switch arms that have multiple labels
            // going into them. For now, the solution is to simply duplicate the same logic into multiple state machines,
            // and we can create separate arms for each of them in a row like this. This isn't ideal, but the post processor
            // would be how one could remove the logic that don't apply to the specific switch case.
            List<CilInstruction> stateTransitionStartInstructions =
                ReplaceInstructionsWithStateMachinesYieldReturn(
                    moveNextIl,
                    start,
                    end,
                    stateMachines,
                    outerStateMachine,
                    contextField,
                    postSwitchStateNumber);

            // The first one is already setup by the game so we just need to change the ones after it.
            for (int j = 1; j < arm.Labels.Count; j++)
                switchArmLabels[arm.Labels[j].Index] = stateTransitionStartInstructions[j].CreateLabel();
        }

        // We reserve one state for ourselves to exit the switch, but after that one until the actual switch end the game
        // used, we need to redirect all of them to the end of the switch so there's no chances of something going in the
        // middle of it. At runtime, no other switch than the one we assigned should be used, this is just to be extra safe.
        for (int i = switchStateNumber + 1; i <= lastSwitchStateNumber; i++)
            stateSwitchLabels[i] = switchEndLabel;

        AddStateMachinesToEmptySwitchArms(
            gameModuleData,
            outerStateMachine,
            innerStateMachinesMethodPrefix,
            moveNextIl,
            switchArmLabels,
            switchEndLabel,
            contextParameter,
            contextField,
            postSwitchStateNumber);

        stateSwitchInstruction.Operand = stateSwitchLabels;
        outerSwitchInstruction.Operand = switchArmLabels;
        moveNextIl.OptimizeMacros();
    }

    private static void AddStateMachinesToEmptySwitchArms(
        GameModuleData gameModuleData,
        StateMachine outerStateMachine,
        string innerStateMachinesMethodPrefix,
        CilInstructionCollection moveNextIl,
        IList<ICilLabel> switchArmLabels,
        ICilLabel switchEndLabel,
        NamedParameter? contextParameter,
        FieldDefinition? contextField,
        int postSwitchStateNumber)
    {
        // This is needed because we clobbered the original label's offset due to patching the other arms.
        moveNextIl.CalculateOffsets();
        IList<IndexedSwitchLabel> emptyArmsLabels = switchArmLabels
            .Select((x, i) => new IndexedSwitchLabel
            {
                Label = x,
                Index = i
            })
            .Where(x => x.Label.Equals(switchEndLabel))
            .ToList();
        foreach (IndexedSwitchLabel emptyArmLabel in emptyArmsLabels)
        {
            StateMachine stateMachine = InnerStateMachineCreator.CreateAndAddInnerStateMachineType(
                outerStateMachine,
                gameModuleData,
                contextParameter is not null ? [contextParameter] : [],
                $"{innerStateMachinesMethodPrefix}{emptyArmLabel.Index}");
            // These arms should do nothing so we just have their MoveNext yield break immediately.
            stateMachine.MoveNextMethod.CilMethodBody = new()
            {
                Instructions =
                {
                    Ldc_I4_0,
                    Ret
                }
            };

            List<CilInstruction> cilInstructions = outerStateMachine.GetYieldReturnToOtherStateMachineInstructions(
                stateMachine,
                contextField is not null ? [contextField] : [],
                postSwitchStateNumber);

            int switchArmLabelIndex = moveNextIl.GetIndexByOffset(switchEndLabel.Offset);
            moveNextIl.InsertRange(switchArmLabelIndex, cilInstructions);
            switchArmLabels[emptyArmLabel.Index] = cilInstructions[0].CreateLabel();
        }
    }

    private static List<SwitchArn> OrganizeSwitchLabelsIntoArms(
        IList<ICilLabel> switchArmLabels,
        ICilLabel switchEndLabel,
        CilInstructionCollection moveNextIl)
    {
        List<(int Offset, List<IndexedSwitchLabel> Labels)> indexedLabelsAndOffset = switchArmLabels
            .Select((x, i) => new IndexedSwitchLabel
            {
                Index = i,
                Label = x
            })
            .Where(x => x.Label.Offset != switchEndLabel.Offset)
            .GroupBy(x => x.Label.Offset)
            .Select(x => (x.Key, x.ToList()))
            .OrderBy(x => x.Key)
            .ToList();

        // We can define that a switch's arm goes from its label's offset to the offset of the preceding instruction of the
        // next label in the switch. This means we can compute all the ending offset by merging 2 of the above collection,
        // one of them 1 element ahead. Since they are ordered by unique offsets, we know the delimiters are accurate for
        // all arms except the last group.
        List<SwitchArn> arms = indexedLabelsAndOffset
            .Zip(
                indexedLabelsAndOffset.Skip(1),
                (current, next) => new SwitchArn
                {
                    Labels = current.Labels,
                    StartInstruction = moveNextIl.GetByOffset(current.Offset)!,
                    EndInstruction =
                        moveNextIl.GetByOffset(moveNextIl[moveNextIl.GetIndexByOffset(next.Offset) - 1].Offset)!
                })
            .ToList();

        (int Offset, List<IndexedSwitchLabel> Labels) lastSwitchArm = indexedLabelsAndOffset[^1];
        arms.Add(
            new SwitchArn
            {
                Labels = lastSwitchArm.Labels,
                StartInstruction = moveNextIl.GetByOffset(lastSwitchArm.Offset)!,
                EndInstruction = moveNextIl.GetByOffset(
                    moveNextIl[moveNextIl.GetIndexByOffset(switchEndLabel.Offset) - 1].Offset)!,
            });

        return arms;
    }

    private static List<CilInstruction> ReplaceInstructionsWithStateMachinesYieldReturn(
        CilInstructionCollection outerIl,
        int ilIndexStartRange,
        int ilIndexEndRange,
        List<StateMachine> innerStateMachines,
        StateMachine outerStateMachine,
        FieldDefinition? stateMachineContextField,
        int stateNumber)
    {
        List<CilInstruction> stateTransitionIl = new();
        List<CilInstruction> stateTransitionStartInstructions = new();
        foreach (StateMachine stateMachine in innerStateMachines)
        {
            List<CilInstruction> cilInstructions = outerStateMachine.GetYieldReturnToOtherStateMachineInstructions(
                stateMachine,
                stateMachineContextField is not null ? [stateMachineContextField] : [],
                stateNumber);
            stateTransitionIl.AddRange(cilInstructions);
            stateTransitionStartInstructions.Add(cilInstructions[0]);
        }

        outerIl.ReplaceRange(ilIndexStartRange, ilIndexEndRange, stateTransitionIl);
        return stateTransitionStartInstructions;
    }
}