using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;

/// <summary>
/// The base class that allows to extract all the arms of a switch from an outer container to an inner one with shared context.
/// </summary>
/// <typeparam name="TOuter">The type of the outer container that has the switch.</typeparam>
/// <typeparam name="TInner">The type of the inner container to extract the switch arms into.</typeparam>
public abstract class SwitchArmsExtractor<TOuter, TInner>
    where TOuter : ILogicContainer
    where TInner : ILogicContainer
{
    /// <summary>
    /// This is an abstraction of what constitutes the code segment forming the arm of a switch. It's possible multiple switch
    /// cases leads to the same arm so this abstraction encodes this by containing offsets to delimit the arm segment and the
    /// labels that leads to it from the switch.
    /// </summary>
    public sealed class SwitchArn
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
    public sealed class IndexedSwitchLabel
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
    /// The outer container.
    /// </summary>
    protected readonly TOuter OuterLogic;

    /// <summary>
    /// A helper to access the <see cref="OuterLogic"/>'s <see cref="CilMethodBody"/>.
    /// </summary>
    protected readonly CilMethodBody OuterBody;

    private readonly GameModuleData _gameModuleData;
    private readonly ILogicExtractorFactory<TOuter, TInner> _logicExtractorFactory;

    /// <summary>
    /// Obtains the IL needed to transfer the control flow from the switch arm to the inner container.
    /// </summary>
    /// <param name="outer">The outer container.</param>
    /// <param name="inner">The inner container.</param>
    /// <param name="switchEndLabel">The label marking the switch's end in the outer container.</param>
    /// <returns>The IL that transfer the control flow from <paramref name="outer"/> to <paramref name="inner"/></returns>
    protected abstract List<CilInstruction> GetOuterToInnerIl(
        TOuter outer,
        TInner inner,
        ICilLabel switchEndLabel);

    /// <summary>
    /// Performs tasks early after basic information about the switch was collected.
    /// </summary>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="outerInitializeContextInstruction">The instruction to insert the initialization code of the context.</param>
    /// <param name="outerSwitchInstruction">The instruction of the switch.</param>
    /// <param name="switchEndLabel">The label marking the end of the switch.</param>
    protected abstract void Setup(
        GameModuleData gameModuleData,
        CilInstruction outerInitializeContextInstruction,
        CilInstruction outerSwitchInstruction,
        ICilLabel switchEndLabel);

    /// <summary>
    /// Performs tasks before ending the extractions.
    /// </summary>
    /// <param name="switchEndLabel">The label marking the end of the switch.</param>
    protected virtual void PostExtraction(ICilLabel switchEndLabel) { return; }

    /// <summary>
    /// Creates an extractor that can extract the logic from each of a switch arms into many inner containers.
    /// </summary>
    /// <param name="gameModuleData">The game module data.</param>
    /// <param name="outerLogic">The outer container that contains the switch.</param>
    /// <param name="logicExtractorFactory">A factory to extract logic for each of the arms.</param>
    protected SwitchArmsExtractor(
        GameModuleData gameModuleData,
        TOuter outerLogic,
        ILogicExtractorFactory<TOuter, TInner> logicExtractorFactory)
    {
        _logicExtractorFactory = logicExtractorFactory;
        _gameModuleData = gameModuleData;
        OuterLogic = outerLogic;
        OuterBody = outerLogic.ReceivingMethod.CilMethodBody!;
    }

    /// <summary>
    /// Extract all the switch arms into inner containers.
    /// </summary>
    /// <param name="innerContainersMethodPrefix">A prefix to place before the case identifier for each of the inner
    /// containers</param>
    /// <param name="outerSwitchInstruction">The switch instruction.</param>
    /// <param name="outerInitializeContextInstruction">The instruction to insert the context initialization code at.</param>
    /// <param name="outerResetContainerInstruction">An instruction that marks the place the code will branch to
    /// if the switch arm where to restart from the beginning of its logic.</param>
    /// <param name="innerContainerPostProcessor">An optional delegate that performs further patching on each of the inner
    /// containers after the logic have been fully extracted from the switch arm.</param>
    public void ExtractSwitchArms(
        string innerContainersMethodPrefix,
        CilInstruction outerSwitchInstruction,
        CilInstruction outerInitializeContextInstruction,
        CilInstruction? outerResetContainerInstruction,
        Action<int, TInner, GameModuleData>? innerContainerPostProcessor)
    {
        CilInstructionCollection moveNextIl = OuterBody.Instructions;
        moveNextIl.ExpandMacros();
        moveNextIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(moveNextIl);

        ilCursor.Index = moveNextIl.GetIndexByOffset(outerSwitchInstruction.Offset);
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.Index++;
        ICilLabel switchEndLabel = (ICilLabel)ilCursor.Current().Operand!;

        Setup(_gameModuleData, outerInitializeContextInstruction, outerSwitchInstruction, switchEndLabel);

        List<SwitchArn> switchArms = OrganizeSwitchLabelsIntoArms(switchArmLabels, switchEndLabel, moveNextIl);

        Dictionary<CilInstruction, TInner> firstInstructionToInnerContainers = new();
        Dictionary<int, LogicExtractor<TOuter, TInner>> labelIndexesToExtractors = new();
        // The first pass creates all the extractor. This allows goto case flow to map even if we encounter an arm
        // we haven't processed yet.
        foreach (SwitchArn arm in switchArms)
        foreach (IndexedSwitchLabel indexedSwitchLabel in arm.Labels)
        {
            LogicExtractor<TOuter, TInner> extractor = _logicExtractorFactory.Create(
                OuterLogic,
                _gameModuleData,
                $"{innerContainersMethodPrefix}{indexedSwitchLabel.Index}",
                arm.StartInstruction,
                arm.EndInstruction);

            labelIndexesToExtractors[indexedSwitchLabel.Index] = extractor;
            firstInstructionToInnerContainers[((CilInstructionLabel)indexedSwitchLabel.Label).Instruction!] =
                extractor.InnerLogic;
        }

        foreach (SwitchArn arm in switchArms)
        {
            moveNextIl.CalculateOffsets();
            Dictionary<int, TInner> offsetsToInnerContainers =
                firstInstructionToInnerContainers.ToDictionary(x => x.Key.Offset, x => x.Value);

            List<TInner> innerContainers = new();
            foreach (IndexedSwitchLabel indexedSwitchArmLabel in arm.Labels)
            {
                TInner innerContainer = labelIndexesToExtractors[indexedSwitchArmLabel.Index].InnerLogic;
                innerContainers.Add(innerContainer);

                labelIndexesToExtractors[indexedSwitchArmLabel.Index].ExtractIl(
                    offsetsToInnerContainers,
                    switchEndLabel,
                    outerResetContainerInstruction,
                    out List<int> usedExceptionHandlerIndexes);

                foreach (int exceptionHandlerIndex in usedExceptionHandlerIndexes.OrderDescending())
                    OuterLogic.ReceivingMethod.CilMethodBody!.ExceptionHandlers.RemoveAt(exceptionHandlerIndex);

                innerContainerPostProcessor?.Invoke(indexedSwitchArmLabel.Index, innerContainer, _gameModuleData);
            }

            int start = moveNextIl.GetIndexByOffset(arm.StartInstruction.Offset);
            int end = moveNextIl.GetIndexByOffset(arm.EndInstruction.Offset);
            // The reason we transfer multiple times in a row is to accomodate switch arms that have multiple cases
            // going into them. For now, the solution is to simply duplicate the same logic into multiple state machines,
            // and we can create separate arms for each of them in a row like this. This isn't ideal, but the post processor
            // would be how one could remove the logic that don't apply to the specific switch cases.
            List<CilInstruction> stateTransitionStartInstructions =
                ReplaceInstructionsWithMultipleReturnToOuterContainer(
                    moveNextIl,
                    start,
                    end,
                    innerContainers,
                    OuterLogic,
                    switchEndLabel);

            // The first one is already setup by the game so we just need to change the ones after it.
            for (int j = 1; j < arm.Labels.Count; j++)
                switchArmLabels[arm.Labels[j].Index] = stateTransitionStartInstructions[j].CreateLabel();
        }

        PostExtraction(switchEndLabel);

        outerSwitchInstruction.Operand = switchArmLabels;
        moveNextIl.OptimizeMacros();
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

    private List<CilInstruction> ReplaceInstructionsWithMultipleReturnToOuterContainer(
        CilInstructionCollection outerIl,
        int ilIndexStartRange,
        int ilIndexEndRange,
        List<TInner> innerContainers,
        TOuter outerContainer,
        ICilLabel switchEndLabel)
    {
        List<CilInstruction> stateTransitionIl = new();
        List<CilInstruction> stateTransitionStartInstructions = new();
        foreach (TInner innerContainer in innerContainers)
        {
            List<CilInstruction> cilInstructions = GetOuterToInnerIl(
                outerContainer,
                innerContainer,
                switchEndLabel);
            stateTransitionIl.AddRange(cilInstructions);
            stateTransitionStartInstructions.Add(cilInstructions[0]);
        }

        outerIl.ReplaceRange(ilIndexStartRange, ilIndexEndRange, stateTransitionIl);
        return stateTransitionStartInstructions;
    }
}