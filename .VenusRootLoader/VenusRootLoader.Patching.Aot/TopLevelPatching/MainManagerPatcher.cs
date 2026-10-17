using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.Factories;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors.SwitchArmsExtractors;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.MethodLogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class MainManagerPatcher : TopLevelTypePatcher
{
    public override string TypeName => "MainManager";

    public override void PatchType(GameModuleData gameModuleData, TypeDefinition type)
    {
        MethodDefinition setTextMethod = type.Methods
            .Where(x => x.Name == "SetText")
            .OrderByDescending(x => x.Signature!.ParameterTypes.Count)
            .First();
        StateMachine setTextStateMachine = new(gameModuleData, setTextMethod);
        Dictionary<int, string> commandsEnum = BuildEnumValueToNameMapping(gameModuleData, "Commands");
        ExtractSetText(gameModuleData, type, setTextStateMachine, commandsEnum);

        MethodDefinition doItemEffectMethod = type.Methods
            .Single(x => x.Name == "DoItemEffect");
        MethodLogicContainer methodContainer = new(doItemEffectMethod);
        Dictionary<int, string> itemUsageEnum = BuildEnumValueToNameMapping(gameModuleData, "ItemUsage");
        ExtractDoItemEffect(gameModuleData, methodContainer, itemUsageEnum);

        MethodDefinition showItemListMethod = type.Methods
            .Single(x => x.Name == "ShowItemList");
        MethodLogicContainer showItemListContainer = new(showItemListMethod);
        ExtractShowItemList(gameModuleData, showItemListContainer);

        MethodDefinition refreshSkillsMethod = type.Methods
            .Single(x => x.Name == "RefreshSkills");
        MethodLogicContainer refreshSkillsContainer = new(refreshSkillsMethod);
        Dictionary<int, string> skillsEnum = BuildEnumValueToNameMapping(gameModuleData, "Skills");
        ExtractRefreshSkills(gameModuleData, refreshSkillsContainer, skillsEnum);
    }

    private static void ExtractSetText(
        GameModuleData gameModuleData,
        TypeDefinition type,
        StateMachine setTextStateMachine,
        Dictionary<int, string> commandsEnum)
    {
        CilInstructionCollection setTextMoveNextIl =
            setTextStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        setTextMoveNextIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(setTextMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Call && x.Operand == type.Methods.Single(y => y.Name == "SetTalk"));
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction commandSwitch = ilCursor.Current();
        int indexCommandSwitch = ilCursor.Index;
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.MatchPrevious(x => x.OpCode == Ldfld);
        CilInstruction beforeCommandSwitch = ilCursor.Current();

        FixWaitCnArmBoundaries(ilCursor, switchArmLabels, setTextMoveNextIl);
        setTextMoveNextIl.CalculateOffsets();
        ilCursor.Index = indexCommandSwitch;
        FixMissingSetTextGotoCases(ilCursor, switchArmLabels, setTextMoveNextIl);

        setTextStateMachine.AssignNewContext(
            "Command",
            [
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("text", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fonttype", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("linebreak", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("dialogue", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tridimensional", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("position", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("cameraoffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("size", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("parent", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("caller", false),

                setTextStateMachine.AddContextFieldFromSpeakableFieldName("speed", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("currentoffset", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("currentline", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("maxlenght", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleeppitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleepvolume", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("langOffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("colorindex", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("sort", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("transferi", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("writen", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ignorenext", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("layer", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("centralize", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("wavy", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("shaky", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("rainbow", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("glitchy", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("skipi", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("end", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("promptmenu", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("minibubble", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tempoverf", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("questboardpromp", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tempevent", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fadeletter", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fontlock", false),
                // TODO: Only affects something on the first 10 iterations of the char loop?
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("initialflip", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ui3d", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("single", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("testing", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("locksize", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("superglitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("dropshadow", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("returnentitycol", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ndd", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ds", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("transfer", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("backbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("textholder", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("textbox", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("windowstyle", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bubbles", true),
                // TODO: Probably should be read only, Blank writes a new blank list while it probably wanted to clear it.
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("buts", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tokenbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleep", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("i", false),
                // TODO: Only Prompt writes to it, but it's suspicious, recheck if this could be read only.
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("command", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("temp", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("com", true)
            ]);

        SwitchArmsStateMachineExtractor setTextSwitchArmsExtractor = new(gameModuleData, setTextStateMachine);
        setTextSwitchArmsExtractor.ExtractSwitchArms(
            commandSwitch,
            beforeCommandSwitch,
            null,
            null,
            commandsEnum,
            "Command");
    }

    private static void FixWaitCnArmBoundaries(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection setTextMoveNextIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[165].Offset);
        int indexSwitchArmStartFromLabel = ilCursor.Index;
        ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
        int indexRealSwitchArmStart = ilCursor.Index + 1;
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        int indexSwitchArmEnd = ilCursor.Index;

        List<CilInstruction> instructionsToMove = setTextMoveNextIl
            .Skip(indexSwitchArmStartFromLabel)
            .Take(indexSwitchArmEnd - indexSwitchArmStartFromLabel + 1)
            .ToList();
        setTextMoveNextIl.RemoveRange(indexSwitchArmStartFromLabel, instructionsToMove.Count);
        setTextMoveNextIl.InsertRange(indexRealSwitchArmStart, instructionsToMove);
        setTextMoveNextIl.Insert(indexSwitchArmEnd + 1, new CilInstruction(Br, instructionsToMove[0].CreateLabel()));
    }

    private static void FixMissingSetTextGotoCases(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection setTextMoveNextIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[179].Offset);
        ilCursor.MatchNext(x => x.OpCode == Stfld);
        ilCursor.Index++;
        setTextMoveNextIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[209].Offset);
        ilCursor.MatchNext(x => x.OpCode == Stfld);
        ilCursor.Index++;
        setTextMoveNextIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
    }

    private static void ExtractDoItemEffect(
        GameModuleData gameModuleData,
        MethodLogicContainer doItemEffectContainer,
        Dictionary<int, string> itemUsageEnum)
    {
        CilInstructionCollection methodIl = doItemEffectContainer.ReceivingMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(methodIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction doItemEffectSwitch = ilCursor.Current();
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.MatchPrevious(x => x.OpCode == Ldarg_0 || x.OpCode == Ldarg);
        CilInstruction beforeDoItemEffectSwitch = ilCursor.Current();

        FixMissingDoItemEffectGotoCases(ilCursor, switchArmLabels, methodIl);

        doItemEffectContainer.AssignNewContext(
            "",
            [
                doItemEffectContainer.AddContextFieldFromArgumentIndexAndLocalIndexes(0, [0], true),
                doItemEffectContainer.AddContextFieldFromArgumentIndex(1, false),
                doItemEffectContainer.AddContextFieldFromArgumentIndex(2, true),
            ]);

        SwitchArmsMethodsExtractor doItemEffectSwitchArmsExtractor = new(
            gameModuleData,
            doItemEffectContainer);
        doItemEffectSwitchArmsExtractor.ExtractSwitchArms(
            doItemEffectSwitch,
            beforeDoItemEffectSwitch,
            null,
            null,
            itemUsageEnum);
    }

    private static void FixMissingDoItemEffectGotoCases(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection doItemEffectIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[8].Offset);
        ilCursor.MatchNext(x => x.OpCode == Call && ((IMethodDefOrRef)x.Operand!).Name == "BadgeIsEquipped");
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Index++;
        doItemEffectIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
    }

    private static void ExtractShowItemList(GameModuleData gameModuleData, MethodLogicContainer showItemListContainer)
    {
        CilInstructionCollection methodIl = showItemListContainer.ReceivingMethod.CilMethodBody!.Instructions;
        methodIl.CalculateOffsets();
        ExtractShowItemListSetup(gameModuleData, showItemListContainer, methodIl);
        methodIl.CalculateOffsets();
        ExtractShowItemListRender(gameModuleData, showItemListContainer, methodIl);
    }

    private static void ExtractShowItemListSetup(
        GameModuleData gameModuleData,
        MethodLogicContainer showItemListContainer,
        CilInstructionCollection methodIl)
    {
        AsmResolverIlCursor ilCursor = new(methodIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction showItemListSetupSwitch = ilCursor.Current();
        int indexSwitch = ilCursor.Index;
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.MatchPrevious(x => x.OpCode == Ldarg_0 || x.OpCode == Ldarg);
        CilInstruction beforeShowItemListSwitch = ilCursor.Current();

        ilCursor.Index = methodIl.GetIndexByOffset(switchArmLabels[5].Offset);
        int startBadArm = ilCursor.Index;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        int endBadArm = ilCursor.Index;
        methodIl.RemoveRange(startBadArm, endBadArm - startBadArm + 1);

        ilCursor.Index = indexSwitch;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        CilInstruction branchToSwitchEnd = ilCursor.Current();
        CilInstructionLabel labelDefault = (CilInstructionLabel)branchToSwitchEnd.Operand!;
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        CilInstructionLabel labelRealSwitchEnd = (CilInstructionLabel)ilCursor.Current().Operand!;

        branchToSwitchEnd.Operand = labelRealSwitchEnd;
        switchArmLabels[5] = labelDefault;

        for (int i = 7; i <= 11; i++)
            switchArmLabels[i] = labelRealSwitchEnd;

        showItemListContainer.AssignNewContext(
            "Setup",
            [
                showItemListContainer.AddContextFieldFromArgumentIndex(0, false),
                showItemListContainer.AddContextFieldFromLocalIndex(0, false),
                showItemListContainer.AddContextFieldFromArgumentIndex(1, true),
                showItemListContainer.AddContextFieldFromArgumentIndex(2, false),
                showItemListContainer.AddContextFieldFromArgumentIndex(3, true),
                showItemListContainer.AddContextFieldFromLocalIndex(2, false)
            ]);

        SwitchArmsMethodsExtractor showItemListArmsExtractor = new(
            gameModuleData,
            showItemListContainer);
        showItemListArmsExtractor.ExtractSwitchArms(
            showItemListSetupSwitch,
            beforeShowItemListSwitch,
            null,
            null,
            [],
            "Setup");
    }

    private static void ExtractShowItemListRender(
        GameModuleData gameModuleData,
        MethodLogicContainer showItemListContainer,
        CilInstructionCollection methodIl)
    {
        AsmResolverIlCursor ilCursor = new(methodIl);

        Dictionary<int, ICilLabel> switchCases = [];
        List<int> ilIndexesToRemove = [];

        ilCursor.MatchNext(x => x.OpCode == Ldstr && (string)x.Operand! == "Bar");
        ilCursor.MatchNext(x => x.OpCode == Ldc_R4 && Math.Abs((float)x.Operand! - -0.15f) < float.Epsilon);
        ilCursor.MatchNext(x => x.IsLdarg());
        int indexBeforeSwitch = ilCursor.Index;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilIndexesToRemove.Add(ilCursor.Index);
        ilIndexesToRemove.Add(ilCursor.Index - 1);

        switchCases.TryAdd(methodIl[ilCursor.Index - 1].GetLdcI4Constant(), methodIl[ilCursor.Index + 1].CreateLabel());
        ilCursor.Index = methodIl.GetIndexByOffset(((ICilLabel)ilCursor.Current().Operand!).Offset);

        ICilLabel switchEndLabel = (ICilLabel)methodIl[ilCursor.Index - 1].Operand!;

        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsConditionalBranch());

        while (ilCursor.Current().Offset <= switchEndLabel.Offset)
        {
            ilIndexesToRemove.Add(ilCursor.Index);
            ilIndexesToRemove.Add(ilCursor.Index - 1);
            ilIndexesToRemove.Add(ilCursor.Index - 2);

            CilOpCode currentOpCode = ilCursor.Current().OpCode;
            if (currentOpCode == Bne_Un || currentOpCode == Bne_Un_S)
            {
                switchCases.TryAdd(
                    methodIl[ilCursor.Index - 1].GetLdcI4Constant(),
                    methodIl[ilCursor.Index + 1].CreateLabel());
            }
            else if (currentOpCode == Beq || currentOpCode == Beq_S)
            {
                switchCases.TryAdd(
                    methodIl[ilCursor.Index - 1].GetLdcI4Constant(),
                    (ICilLabel)ilCursor.Current().Operand!);
                ilCursor.Index++;
                ilCursor.MatchNext(x => x.IsConditionalBranch());
                continue;
            }
            else if (currentOpCode == Blt || currentOpCode == Blt_S)
            {
                int lowerBound = methodIl[ilCursor.Index - 1].GetLdcI4Constant();
                ilCursor.Index++;
                ilCursor.MatchNext(x => x.IsConditionalBranch());
                ilIndexesToRemove.Add(ilCursor.Index);
                ilIndexesToRemove.Add(ilCursor.Index - 1);
                ilIndexesToRemove.Add(ilCursor.Index - 2);
                int upperBound = methodIl[ilCursor.Index - 1].GetLdcI4Constant();

                CilOpCode upperBoundOpCode = ilCursor.Current().OpCode;
                if (upperBoundOpCode == Bgt || upperBoundOpCode == Bgt_S)
                {
                    ICilLabel label = methodIl[ilCursor.Index + 1].CreateLabel();
                    for (int i = lowerBound; i <= upperBound; i++)
                        switchCases.TryAdd(i, label);
                }
                else if (upperBoundOpCode == Ble || upperBoundOpCode == Ble_S)
                {
                    ICilLabel label = (ICilLabel)ilCursor.Current().Operand!;
                    for (int i = lowerBound; i <= upperBound; i++)
                        switchCases.TryAdd(i, label);

                    ilCursor.Index++;
                    ilCursor.MatchNext(x => x.IsConditionalBranch());
                    continue;
                }
            }

            ilCursor.Index = methodIl.GetIndexByOffset(((ICilLabel)ilCursor.Current().Operand!).Offset);
            ilCursor.MatchNext(x => x.IsConditionalBranch());
        }

        foreach (int i in ilIndexesToRemove.OrderDescending())
            methodIl.RemoveAt(i);

        List<ICilLabel> switchLabels = [];
        int firstCaseKey = switchCases.MinBy(x => x.Key).Key;
        int lastCaseKey = switchCases.MaxBy(x => x.Key).Key + 1;
        for (int i = firstCaseKey; i <= lastCaseKey; i++)
            switchLabels.Add(switchCases.GetValueOrDefault(i, switchEndLabel));

        methodIl.InsertRange(
            indexBeforeSwitch + 1,
            [
                CilInstruction.CreateLdcI4(-3),
                new(Sub),
                new(Switch, switchLabels),
                new(Br, switchEndLabel)
            ]);
        ilCursor.Index = indexBeforeSwitch;

        ilCursor.MatchNext(x => x.OpCode == Switch);
        CilInstruction showItemListRenderSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdarg());
        CilInstruction beforeShowItemListRenderSwitch = ilCursor.Current();

        showItemListContainer.AssignNewContext(
            "Render",
            [
                showItemListContainer.AddContextFieldFromArgumentIndex(0, true),
                showItemListContainer.AddContextFieldFromArgumentIndex(1, true),
                showItemListContainer.AddContextFieldFromArgumentIndex(2, true),
                showItemListContainer.AddContextFieldFromArgumentIndex(3, true),
                showItemListContainer.AddContextFieldFromLocalIndex(0, true),
                showItemListContainer.AddContextFieldFromLocalIndex(1, true),
                showItemListContainer.AddContextFieldFromLocalIndex(2, true),
                showItemListContainer.AddContextFieldFromLocalIndex(9, false),
                showItemListContainer.AddContextFieldFromLocalIndex(10, false),
                showItemListContainer.AddContextFieldFromLocalIndex(11, true),
                showItemListContainer.AddContextFieldFromLocalIndex(15, true),
                showItemListContainer.AddContextFieldFromLocalIndex(16, true),
                showItemListContainer.AddContextFieldFromLocalIndex(17, false),
                showItemListContainer.AddContextFieldFromLocalIndex(18, false),
                showItemListContainer.AddContextFieldFromLocalIndex(19, false)
            ]);

        SwitchArmsMethodsExtractor showItemListArmsExtractor = new(
            gameModuleData,
            showItemListContainer);
        showItemListArmsExtractor.ExtractSwitchArms(
            showItemListRenderSwitch,
            beforeShowItemListRenderSwitch,
            null,
            null,
            [],
            "Render");
    }

    private static void ExtractRefreshSkills(
        GameModuleData gameModuleData,
        MethodLogicContainer refreshSkillsContainer,
        Dictionary<int, string> skillsEnum)
    {
        CilInstructionCollection methodIl = refreshSkillsContainer.ReceivingMethod.CilMethodBody!.Instructions;
        methodIl.ExpandMacros();
        RefreshSkillsSpreadBattleNullChecks(methodIl);
        RefreshSkillsInsertDummyConditions(methodIl);
        RefreshSkillsSpreadRoyalDecreeConditions(methodIl);
        ExtractRefreshSkillsPredicates(gameModuleData, refreshSkillsContainer, methodIl, skillsEnum);
        methodIl.OptimizeMacros();
    }

    private static void RefreshSkillsSpreadBattleNullChecks(CilInstructionCollection methodIl)
    {
        AsmResolverIlCursor ilCursor = new(methodIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        List<ICilLabel> switchLabels = (List<ICilLabel>)ilCursor.Current().Operand!;
        foreach (ICilLabel switchLabel in switchLabels)
        {
            methodIl.CalculateOffsets();
            ilCursor.Index = methodIl.GetIndexByOffset(switchLabel.Offset);

            List<CilInstruction> battleNullCheck = [ilCursor.Current()];
            while (!ilCursor.Current().IsConditionalBranch())
            {
                ilCursor.Index++;
                battleNullCheck.Add(ilCursor.Current());
            }

            int firstLabelInstruction = ilCursor.Index;
            ICilLabel lastLabel = (ICilLabel)ilCursor.Current().Operand!;

            bool isFirst = true;
            while (ilCursor.Current().Offset < lastLabel.Offset)
            {
                int startCondition = ilCursor.Index;
                CilInstruction firstInstructionCondition = ilCursor.Current();
                ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
                ilCursor.Index++;
                CilInstructionLabel newLabel = new(ilCursor.Current());

                if (isFirst)
                {
                    methodIl[firstLabelInstruction].Operand = newLabel;
                }
                else
                {
                    List<CilInstruction> newBattleNullCheck = [];
                    foreach (CilInstruction instruction in battleNullCheck)
                        newBattleNullCheck.Add(new(instruction.OpCode, instruction.Operand));

                    methodIl.ReplaceRange(
                        startCondition,
                        startCondition,
                        [
                            .. newBattleNullCheck,
                            new(firstInstructionCondition.OpCode, firstInstructionCondition.Operand)
                        ]);
                    methodIl[startCondition + newBattleNullCheck.Count - 1].Operand = newLabel;
                    ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
                    ilCursor.Index++;
                }

                isFirst = false;
            }
        }
    }

    private static void RefreshSkillsInsertDummyConditions(CilInstructionCollection methodIl)
    {
        methodIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(methodIl);
        ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");

        while (ilCursor.Index >= 0)
        {
            int indexAdd = ilCursor.Index;
            ilCursor.Index++;
            CilInstruction instructionAfter = ilCursor.Current();
            int expectedOffset = ilCursor.Current().Offset;
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            ICilLabel label = (ICilLabel)ilCursor.Current().Operand!;
            if (!instructionAfter.IsUnconditionalBranch() && expectedOffset != label.Offset)
            {
                ilCursor.Index = indexAdd - 1;
                ilCursor.MatchPrevious(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
                ilCursor.Index++;
                CilInstruction instructionBefore = ilCursor.Current();

                methodIl.ReplaceRange(
                    ilCursor.Index,
                    ilCursor.Index,
                    [
                        new(Ldc_I4_1),
                        new(Brfalse, new CilInstructionLabel(instructionAfter)),
                        new(instructionBefore.OpCode, instructionBefore.Operand)
                    ]);
            }

            ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
            ilCursor.Index++;
            ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
        }
    }

    private static void RefreshSkillsSpreadRoyalDecreeConditions(CilInstructionCollection methodIl)
    {
        methodIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(methodIl);

        List<CilInstruction> royalDecreeConditions = [ilCursor.Current()];
        while (!ilCursor.Current().IsStloc())
        {
            ilCursor.Index++;
            royalDecreeConditions.Add(ilCursor.Current());
        }

        CilLocalVariable local = (CilLocalVariable)ilCursor.Current().Operand!;
        methodIl.RemoveRange(0, royalDecreeConditions.Count);

        ilCursor.MatchNext(x => x.IsLdloc() && x.Operand == local);
        while (ilCursor.Index >= 0)
        {
            List<CilInstruction> royalDecreeConditionsClone = [];
            foreach (CilInstruction instruction in royalDecreeConditions)
                royalDecreeConditionsClone.Add(new(instruction.OpCode, instruction.Operand));
            foreach (CilInstruction instruction in royalDecreeConditionsClone)
            {
                if (instruction.IsConditionalBranch())
                    instruction.Operand = new CilInstructionLabel(royalDecreeConditionsClone[^2]);
                else if (instruction.IsUnconditionalBranch())
                    instruction.Operand = new CilInstructionLabel(royalDecreeConditionsClone[^1]);
            }

            CilInstruction instructionAfter = ilCursor.Current();
            methodIl.ReplaceRange(
                ilCursor.Index,
                ilCursor.Index,
                [
                    .. royalDecreeConditionsClone,
                    new(instructionAfter.OpCode, instructionAfter.Operand)
                ]);

            ilCursor.MatchNext(x => x.IsLdloc() && x.Operand == local);
            ilCursor.Index++;
            ilCursor.MatchNext(x => x.IsLdloc() && x.Operand == local);
        }
    }

    private static void ExtractRefreshSkillsPredicates(
        GameModuleData gameModuleData,
        MethodLogicContainer refreshSkillsContainer,
        CilInstructionCollection methodIl,
        Dictionary<int, string> skillsEnum)
    {
        methodIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(methodIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        List<ICilLabel> labelsSwitch = (List<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.Index++;
        ICilLabel switchEndLabel = (ICilLabel)ilCursor.Current().Operand!;
        int offsetCommit = switchEndLabel.Offset;
        ilCursor.MatchPrevious(x => x.IsLdloc());
        int offsetInit = ilCursor.Current().Offset;

        refreshSkillsContainer.AssignNewContext(
            "",
            [
                refreshSkillsContainer.AddContextFieldFromLocalIndex(1, true, "playerDataIndex")
            ]);
        refreshSkillsContainer.PatchContextIntoContainer(gameModuleData, offsetInit, offsetCommit);

        ilCursor.Index = methodIl.GetIndexByOffset(labelsSwitch[0].Offset);

        Dictionary<int, MethodDefinition> innerMethodBySkillIds = [];
        int indexStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
        while (ilCursor.Index >= 0)
        {
            methodIl.CalculateOffsets();
            int skillId = methodIl[ilCursor.Index - 1].GetLdcI4Constant();
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            int indexEnd = ilCursor.Index;
            CilInstruction lastInstruction = ilCursor.Current();
            ICilLabel labelContinue = (ICilLabel)lastInstruction.Operand!;

            if (!innerMethodBySkillIds.TryGetValue(skillId, out MethodDefinition? innerMethod))
            {
                MethodLogicExtractor extractor = new(
                    refreshSkillsContainer,
                    new InnerMethodFactory(),
                    gameModuleData,
                    $"RefreshSkills_Predicate_{skillsEnum[skillId]}",
                    methodIl[indexStart],
                    methodIl[indexEnd]);
                extractor.ExtractIl([], labelContinue, null, out _);
                MethodDefinition newInnerMethod = extractor.InnerLogic.ReceivingMethod;
                PostProcessRefreshSkillsPredicate(gameModuleData, newInnerMethod);
                innerMethod = newInnerMethod;
                innerMethodBySkillIds.Add(skillId, innerMethod);
            }

            methodIl.ReplaceRange(
                indexStart,
                indexEnd,
                [
                    new(Ldloc, refreshSkillsContainer.ContextLocal),
                    new(Call, innerMethod),
                    new(Brfalse, labelContinue)
                ]);

            ilCursor.Index = indexStart;
            ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
            ilCursor.Index++;
            if (ilCursor.Current().IsUnconditionalBranch())
                ilCursor.Index++;
            indexStart = ilCursor.Index;
            ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
        }
    }

    private static void PostProcessRefreshSkillsPredicate(
        GameModuleData gameModuleData,
        MethodDefinition newInnerMethod)
    {
        CilInstructionCollection il = newInnerMethod.CilMethodBody!.Instructions;
        il.ExpandMacros();
        il.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(il);

        ilCursor.Index = il.Count - 1;
        if (il[ilCursor.Index - 2].OpCode == Brfalse)
        {
            il.RemoveRange(ilCursor.Index - 2, 2);
            ilCursor.Index = il.Count - 1;
        }
        else
        {
            il.ReplaceRange(ilCursor.Index, ilCursor.Index, [new(Ldc_I4_0), new(Ret)]);
            ilCursor.Index = il.Count - 1;
            ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
            il[ilCursor.Index].ReplaceWith(Ret);
            il.Insert(ilCursor.Index, Ldc_I4_1);
        }

        ilCursor.Index--;
        ilCursor.MatchPrevious(x => x.OpCode == Ret);
        while (ilCursor.Index >= 0)
        {
            il.ReplaceRange(ilCursor.Index, ilCursor.Index, [new(Ldc_I4_0), new(Ret)]);
            ilCursor.MatchPrevious(x => x.OpCode == Ret);
        }

        newInnerMethod.Signature!.ReturnType = gameModuleData.Module.CorLibTypeFactory.Boolean;
    }
}