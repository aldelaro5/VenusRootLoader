using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
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

        MethodDefinition checkAchievementMethod = type.Methods
            .Single(x => x.Name == "CheckAchievement");
        MethodLogicContainer checkAchievementContainer = new(checkAchievementMethod);
        ExtractCheckAchievementContainer(gameModuleData, checkAchievementContainer);
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
                doItemEffectContainer.GetContextFieldFromArgumentIndexAndLocalIndexes(0, [0], true),
                doItemEffectContainer.GetContextFieldFromArgumentIndex(1, false),
                doItemEffectContainer.GetContextFieldFromArgumentIndex(2, true),
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
        methodIl.ExpandMacros();
        methodIl.CalculateOffsets();
        ExtractShowItemListDescriptionRender(gameModuleData, showItemListContainer, methodIl);
        methodIl.OptimizeMacros();
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
                showItemListContainer.GetContextFieldFromArgumentIndex(0, false),
                showItemListContainer.GetContextFieldFromLocalIndex(0, false),
                showItemListContainer.GetContextFieldFromArgumentIndex(1, true),
                showItemListContainer.GetContextFieldFromArgumentIndex(2, false),
                showItemListContainer.GetContextFieldFromArgumentIndex(3, true),
                showItemListContainer.GetContextFieldFromLocalIndex(2, false)
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
                showItemListContainer.GetContextFieldFromArgumentIndex(0, true),
                showItemListContainer.GetContextFieldFromArgumentIndex(1, true),
                showItemListContainer.GetContextFieldFromArgumentIndex(2, true),
                showItemListContainer.GetContextFieldFromArgumentIndex(3, true),
                showItemListContainer.GetContextFieldFromLocalIndex(0, true),
                showItemListContainer.GetContextFieldFromLocalIndex(1, true),
                showItemListContainer.GetContextFieldFromLocalIndex(2, true),
                showItemListContainer.GetContextFieldFromLocalIndex(9, false),
                showItemListContainer.GetContextFieldFromLocalIndex(10, false),
                showItemListContainer.GetContextFieldFromLocalIndex(11, true),
                showItemListContainer.GetContextFieldFromLocalIndex(15, true),
                showItemListContainer.GetContextFieldFromLocalIndex(16, true),
                showItemListContainer.GetContextFieldFromLocalIndex(17, false),
                showItemListContainer.GetContextFieldFromLocalIndex(18, false),
                showItemListContainer.GetContextFieldFromLocalIndex(19, false)
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

    private static void ExtractShowItemListDescriptionRender(
        GameModuleData gameModuleData,
        MethodLogicContainer showItemListContainer,
        CilInstructionCollection methodIl)
    {
        AsmResolverIlCursor ilCursor = new(methodIl);
        showItemListContainer.AssignNewContext("", []);

        ilCursor.MatchNext(x => x.OpCode == Call && x.Operand is IMethodDefOrRef method && method.Name == "Create9Box");
        ilCursor.MatchPrevious(x => x.IsConditionalBranch());
        ICilLabel labelEnd = (ICilLabel)ilCursor.Current().Operand!;

        // First, we extract the default description box renderer into a new method so we can spread a call to it.
        ilCursor.Index++;
        int indexStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        int indexEnd = ilCursor.Index;
        MethodLogicExtractor extractor = new(
            showItemListContainer,
            new InnerMethodFactory(),
            [
                showItemListContainer.GetLogicParameterFromLocalIndex(62),
                showItemListContainer.GetLogicParameterFromLocalIndex(63),
                showItemListContainer.GetLogicParameterFromArgumentIndex(3)
            ],
            gameModuleData.Module.CorLibTypeFactory.Void,
            gameModuleData,
            "ShowItemList_RenderDefaultDescriptionBox",
            methodIl[indexStart],
            methodIl[indexEnd]);
        extractor.ExtractIl(
            [],
            new Dictionary<int, List<CilInstruction>>
            {
                [labelEnd.Offset] = [],
            },
            null,
            out _);

        // Remove the logic of the default renderer since we don't need it anymore as it was just extracted to a method.
        ilCursor.Index = indexStart;
        ilCursor.MatchPrevious(x => x.IsLdloc());
        methodIl.ReplaceRange(ilCursor.Index, indexEnd, [new(Br, labelEnd)]);
        CilInstruction instructionEnd = ilCursor.Current();

        ilCursor.MatchPrevious(x =>
            x.OpCode == Ldsfld && x.Operand is FieldDefinition field && field.Name == "listdescbox");
        ilCursor.MatchNext(x => x.IsLdarg() && x.Operand is Parameter { MethodSignatureIndex: 0 });
        int indexStartChain = ilCursor.Index;

        // We remove an stloc to the local that was supposed to store if we are going to render using the default renderer.
        // Not only this code would get in the way later, we want to get rid of this local usage anyway since we want to
        // spread the rendering check.
        ilCursor.MatchNext(x => x.IsStloc());
        int indexStoreLocalFlagEnd = ilCursor.Index;
        CilInstruction instructionAfterStoreLocalFlag = methodIl[indexStoreLocalFlagEnd + 1];
        ilCursor.MatchPrevious(x => x.IsLdcI4());
        methodIl.ReplaceRange(
            ilCursor.Index,
            indexStoreLocalFlagEnd + 1,
            [
                new(instructionAfterStoreLocalFlag.OpCode, instructionAfterStoreLocalFlag.Operand)
            ]);

        // Next, we want to take a shared check for having a valid option selected and spread it.
        ilCursor.MatchNext(x => x.OpCode == Ldsfld && x.Operand is FieldDefinition field && field.Name == "listvar");
        int indexInnerLogicCheckStart = ilCursor.Index;
        ilCursor.MatchPrevious(x => x.IsConditionalBranch());
        CilInstruction instructionLastBranchInnerLogic = ilCursor.Current();
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ICilLabel labelInnerLogicEnd = (ICilLabel)ilCursor.Current().Operand!;
        int indexInnerLogicCheckEnd = ilCursor.Index;

        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        ilCursor.Index = indexInnerLogicCheckStart;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        while (ilCursor.Current().Offset < labelInnerLogicEnd.Offset)
        {
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            ilCursor.Index++;

            // Spreading the option selected check.
            InsertCloneWithLabelStealing(
                methodIl,
                ilCursor,
                indexInnerLogicCheckStart,
                indexInnerLogicCheckEnd,
                labelEnd);

            ilCursor.MatchNext(x => x.IsUnconditionalBranch());

            // Spreading the method call we extracted earlier.
            methodIl.InsertRange(
                ilCursor.Index,
                [
                    new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[62]),
                    new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[63]),
                    new(Ldarg, showItemListContainer.ReceivingMethod.Parameters[3]),
                    new(Call, extractor.InnerLogic.ReceivingMethod)
                ]);

            ilCursor.MatchNext(x => x.IsUnconditionalBranch());
            // While we're spreading the check, we take this opportunity to normalize the end br so they all go to the end.
            ilCursor.Current().Operand = labelEnd;
            ilCursor.Index++;
            ilCursor.MatchNext(x => x.IsUnconditionalBranch());
            ilCursor.Index++;
        }

        // The last applicable block is a bit of an outlier so we process it separately.
        ilCursor.Index -= 2;
        ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
        ilCursor.Index++;

        InsertCloneWithLabelStealing(
            methodIl,
            ilCursor,
            indexInnerLogicCheckStart,
            indexInnerLogicCheckEnd,
            labelEnd);

        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        ilCursor.Current().Operand = labelEnd;

        methodIl.InsertRange(
            ilCursor.Index,
            [
                new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[62]),
                new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[63]),
                new(Ldarg, showItemListContainer.ReceivingMethod.Parameters[3]),
                new(Call, extractor.InnerLogic.ReceivingMethod)
            ]);

        // We can now remove the original option selected check.
        CilInstruction firstInstructionAfterLogicCheck = methodIl[indexInnerLogicCheckEnd + 1];
        methodIl.ReplaceRange(
            indexInnerLogicCheckStart,
            indexInnerLogicCheckEnd + 1,
            [new(firstInstructionAfterLogicCheck.OpCode, firstInstructionAfterLogicCheck.Operand)]);

        // We can now remove the set local flag logic that is no longer needed and is in the way.
        methodIl.CalculateOffsets();
        ilCursor.Index = methodIl.GetIndexByOffset(labelInnerLogicEnd.Offset);
        int indexSetFlagStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        int indexSetFlagEnd = ilCursor.Index;
        CilInstruction firstInstructionAfterSetFlag = methodIl[indexSetFlagEnd + 1];
        methodIl.ReplaceRange(
            indexSetFlagStart,
            indexSetFlagEnd + 1,
            [new(firstInstructionAfterSetFlag.OpCode, firstInstructionAfterSetFlag.Operand)]);
        ilCursor.Index = indexSetFlagStart;
        instructionLastBranchInnerLogic.Operand = ilCursor.Current().CreateLabel();

        // For the other blocks, we also need to spread the method call, but also a stloc so the second parameter of the
        // method defaults to empty strings.
        methodIl.CalculateOffsets();
        while (ilCursor.Current().Offset < instructionEnd.Offset)
        {
            ICilLabel labelAfter = null!;
            int indexArmStart = 0;
            // All of this is just to find precisely where the block starts since we have to follow the sub if chain until
            // we can't find a followup to it.
            while (ilCursor.Index >= 0 && ilCursor.Current().Offset < instructionEnd.Offset)
            {
                ilCursor.MatchNext(x => x.IsConditionalBranch());
                labelAfter = (ICilLabel)ilCursor.Current().Operand!;
                ilCursor.Index++;
                indexArmStart = ilCursor.Index;
                int labelAfterOffset = labelAfter.Offset;
                ilCursor.MatchNext(x => x.IsConditionalBranch()
                                        && x.Operand is ICilLabel label
                                        && label.Offset == labelAfterOffset);
            }

            ilCursor.Index = methodIl.GetIndexByOffset(labelAfter.Offset);
            bool isLast = false;
            // The last one ends with a br to the end so we want to stay on it.
            if (ilCursor.Current().Offset == instructionEnd.Offset)
                isLast = true;
            else
                ilCursor.Index--;

            methodIl.InsertRange(
                ilCursor.Index,
                [
                    new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[62]),
                    new(Ldloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[63]),
                    new(Ldarg, showItemListContainer.ReceivingMethod.Parameters[3]),
                    new(Call, extractor.InnerLogic.ReceivingMethod)
                ]);
            if (isLast)
            {
                // The last one has 2 branches that needs to lead to out method call so we need to relabel the br.
                ICilLabel newArmEnd = ilCursor.Current().CreateLabel();
                ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
                ilCursor.Current().Operand = newArmEnd;
                ilCursor.Index--;
                ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
                ilCursor.Current().Operand = newArmEnd;
            }
            else
            {
                ilCursor.MatchNext(x => x.IsUnconditionalBranch());
                ilCursor.Current().Operand = labelEnd;
            }

            CilInstruction instructionArmStart = methodIl[indexArmStart];
            methodIl.ReplaceRange(
                indexArmStart,
                indexArmStart,
                [
                    new(Ldstr, ""),
                    new(Stloc, showItemListContainer.ReceivingMethod.CilMethodBody!.LocalVariables[63]),
                    new(instructionArmStart.OpCode, instructionArmStart.Operand)
                ]);

            methodIl.CalculateOffsets();
            ilCursor.Index = methodIl.GetIndexByOffset(labelAfter.Offset);
        }

        // We can now start to move code. We start by moving the quests list types logic up and invert its logic so it no
        // longer introduce nesting.
        int indexQuestTypesStart = ilCursor.Index + 1;
        ilCursor.Index = methodIl.GetIndexByOffset(labelEnd.Offset);
        int indexQuestTypesEnd = ilCursor.Index - 1;
        List<CilInstruction> cloneQuestTypes = [];
        for (int i = indexQuestTypesStart; i <= indexQuestTypesEnd; i++)
            cloneQuestTypes.Add(methodIl[i]);
        cloneQuestTypes.Add(new(Br, labelEnd));
        methodIl.RemoveRange(indexQuestTypesStart, indexQuestTypesEnd - indexQuestTypesStart + 1);

        ilCursor.Index = indexStartChain;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ICilLabel labelNonQuestTypes = (ICilLabel)ilCursor.Current().Operand!;
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Current().ReplaceWith(Bgt, labelNonQuestTypes);
        ilCursor.Index++;
        methodIl.InsertRange(ilCursor.Index, cloneQuestTypes);

        methodIl.CalculateOffsets();
        ilCursor.Index = methodIl.GetIndexByOffset(labelNonQuestTypes.Offset);
        int indexBadIfsStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.OpCode == Bne_Un);
        int indexBadIfsEnd = ilCursor.Index;

        // We'll need a copy of the first part for later since it's the default block conditions, but we need to relabel it.
        ilCursor.MatchPrevious(x => x.OpCode == Blt || x.OpCode == Blt_Un);
        List<CilInstruction> clonedDefaultIf = methodIl.CloneRange(indexBadIfsStart, ilCursor.Index);
        AsmResolverIlCursor subCursor = new(clonedDefaultIf);
        subCursor.MatchNext(x => x.IsConditionalBranch());
        subCursor.Current().Operand = labelEnd;
        subCursor.Index++;
        subCursor.MatchNext(x => x.IsConditionalBranch());
        // We need to change this so it only includes 0 to 2 because 3 was handled already and 4 to 8 aren't used.
        subCursor.Current().ReplaceWith(Bge, labelEnd);
        subCursor.MatchPrevious(x => x.IsLdcI4());
        subCursor.Current().Operand = 3;

        // Now we can remove the whole if chain.
        ilCursor.Index = indexBadIfsEnd + 1;
        CilInstruction instructionAfterBadIfs = ilCursor.Current();
        methodIl.ReplaceRange(
            indexBadIfsStart,
            indexBadIfsEnd + 1,
            [
                new(instructionAfterBadIfs.OpCode, instructionAfterBadIfs.Operand)
            ]);

        // We now need to move some specific list type handlers up to make it easier later to change the last handler
        // conditions.
        ilCursor.Index = indexBadIfsStart;
        ilCursor.MatchNext(x => x.IsLdcI4() && x.GetLdcI4Constant() == -3);
        ilCursor.MatchPrevious(x => x.IsLdarg());
        int indexRestStart = ilCursor.Index;
        ilCursor.Index = methodIl.Count - 1;
        ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
        int indexRestEnd = ilCursor.Index;
        List<CilInstruction> cloneRest = [];
        for (int i = indexRestStart; i <= indexRestEnd; i++)
            cloneRest.Add(methodIl[i]);
        methodIl.RemoveRange(indexRestStart, indexRestEnd - indexRestStart + 1);
        ilCursor.Index = indexRestStart - 1;
        ilCursor.Current().Operand = methodIl[indexRestStart].CreateLabel();

        ilCursor.Index = indexBadIfsStart;
        CilInstruction instructionStartRest = methodIl[ilCursor.Index];
        CilInstruction newInstructionStartRest = new(instructionStartRest.OpCode, instructionStartRest.Operand);
        methodIl.ReplaceRange(
            ilCursor.Index,
            ilCursor.Index,
            [
                .. cloneRest,
                newInstructionStartRest
            ]);

        // Since we moved code that was below upward, we need to relabel its last branching so it connects.
        ICilLabel connectingLabel = newInstructionStartRest.CreateLabel();
        ilCursor.MatchNext(x => x.IsLdcI4() && x.GetLdcI4Constant() == 26);
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Current().Operand = connectingLabel;

        // Remove some code that is unreachable because it checks for a list type that was handled prior.
        ilCursor.Index = methodIl.Count - 1;
        ilCursor.MatchPrevious(x => x.IsLdcI4() && x.GetLdcI4Constant() == 33);
        ilCursor.MatchPrevious(x => x.IsLdarg());
        int indexUnreachableStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ICilLabel labelAfterUnreachable = (ICilLabel)ilCursor.Current().Operand!;
        methodIl.CalculateOffsets();
        ilCursor.Index = methodIl.GetIndexByOffset(labelAfterUnreachable.Offset);
        CilInstruction instructionAfterUnreachable = ilCursor.Current();
        ilCursor.Index--;
        int indexUnreachableEnd = ilCursor.Index;
        methodIl.ReplaceRange(
            indexUnreachableStart,
            indexUnreachableEnd + 1,
            [
                new(instructionAfterUnreachable.OpCode, instructionAfterUnreachable.Operand)
            ]);

        // We can now put back the default arm conditions.
        ilCursor.Index = indexUnreachableStart;
        CilInstruction instructionDefaultArm = ilCursor.Current();
        methodIl.ReplaceRange(
            ilCursor.Index,
            ilCursor.Index,
            [
                .. clonedDefaultIf,
                new(instructionDefaultArm.OpCode, instructionDefaultArm.Operand)
            ]);
    }

    private static void InsertCloneWithLabelStealing(
        CilInstructionCollection methodIl,
        AsmResolverIlCursor ilCursor,
        int indexInnerLogicCheckStart,
        int indexInnerLogicCheckEnd,
        ICilLabel labelEnd)
    {
        List<CilInstruction> cloneIl = methodIl.CloneRange(indexInnerLogicCheckStart, indexInnerLogicCheckEnd);
        cloneIl[^1].Operand = labelEnd;
        CilInstruction firstInstruction = ilCursor.Current();

        methodIl.ReplaceRange(
            ilCursor.Index,
            ilCursor.Index,
            [
                .. cloneIl,
                new(firstInstruction.OpCode, firstInstruction.Operand)
            ]);
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
            else if (ilCursor.Current().OpCode == Blt)
            {
                // This means this is a playerlevel comparison which we want to exclude from the predicates. To do this,
                // we'll insert a dummy condition after it and have the extractor only take this part.
                ilCursor.Index++;
                methodIl.InsertRange(
                    ilCursor.Index,
                    [
                        new(Ldc_I4_1),
                        new(Brfalse, new CilInstructionLabel(instructionAfter))
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
                refreshSkillsContainer.GetContextFieldFromLocalIndex(1, true, "playerDataIndex")
            ]);
        refreshSkillsContainer.PatchContextIntoContainer(gameModuleData, offsetInit, offsetCommit);

        ilCursor.Index = methodIl.GetIndexByOffset(labelsSwitch[0].Offset);

        Dictionary<int, MethodDefinition> innerMethodBySkillIds = [];
        int indexStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "Add");
        while (ilCursor.Index >= 0)
        {
            int skillId = methodIl[ilCursor.Index - 1].GetLdcI4Constant();
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            // This is a playerlevel comparison so we skip that portion in the extractor.
            if (methodIl[ilCursor.Index - 2].OpCode == Blt)
                indexStart = ilCursor.Index - 1;
            CilInstruction lastInstruction = ilCursor.Current();
            ICilLabel labelContinue = (ICilLabel)lastInstruction.Operand!;
            ilCursor.Index++;
            methodIl.Insert(ilCursor.Index, Br, methodIl[ilCursor.Index].CreateLabel());
            int indexEnd = ilCursor.Index;

            if (!innerMethodBySkillIds.TryGetValue(skillId, out MethodDefinition? innerMethod))
            {
                MethodLogicExtractor extractor = new(
                    refreshSkillsContainer,
                    new InnerMethodFactory(),
                    [],
                    gameModuleData.Module.CorLibTypeFactory.Boolean,
                    gameModuleData,
                    $"RefreshSkills_Predicate_{skillsEnum[skillId]}",
                    methodIl[indexStart],
                    methodIl[indexEnd]);
                methodIl.CalculateOffsets();
                extractor.ExtractIl(
                    [],
                    new Dictionary<int, List<CilInstruction>>
                    {
                        [labelContinue.Offset] = [new(Ldc_I4_0)],
                        [methodIl[indexEnd + 1].Offset] = [new(Ldc_I4_1)]
                    },
                    null,
                    out _);
                innerMethod = extractor.InnerLogic.ReceivingMethod;
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

    private static void ExtractCheckAchievementContainer(
        GameModuleData gameModuleData,
        MethodLogicContainer checkAchievementContainer)
    {
        CilInstructionCollection methodIl = checkAchievementContainer.ReceivingMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(methodIl);

        methodIl.ExpandMacros();
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Index++;
        int indexStart = ilCursor.Index;

        ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "UpdateJounal");
        while (ilCursor.Index >= 0)
        {
            if (methodIl[ilCursor.Index - 2].GetLdcI4Constant() != 3)
            {
                ilCursor.Index++;
                ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "UpdateJounal");
                continue;
            }

            int recordId = methodIl[ilCursor.Index - 1].GetLdcI4Constant();
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            CilInstruction lastInstruction = ilCursor.Current();
            ICilLabel labelContinue = (ICilLabel)lastInstruction.Operand!;
            ilCursor.Index++;
            methodIl.Insert(ilCursor.Index, Br, methodIl[ilCursor.Index].CreateLabel());
            int indexEnd = ilCursor.Index;

            MethodLogicExtractor extractor = new(
                checkAchievementContainer,
                new InnerMethodFactory(),
                [],
                gameModuleData.Module.CorLibTypeFactory.Boolean,
                gameModuleData,
                $"CheckAchievement_Predicate_{recordId}",
                methodIl[indexStart],
                methodIl[indexEnd]);
            methodIl.CalculateOffsets();
            extractor.ExtractIl(
                [],
                new Dictionary<int, List<CilInstruction>>
                {
                    [labelContinue.Offset] = [new(Ldc_I4_0)],
                    [methodIl[indexEnd + 1].Offset] = [new(Ldc_I4_1)]
                },
                null,
                out _);
            MethodDefinition innerMethod = extractor.InnerLogic.ReceivingMethod;

            methodIl.ReplaceRange(
                indexStart,
                indexEnd,
                [
                    new(Ldarg_0),
                    new(Call, innerMethod),
                    new(Brfalse, labelContinue)
                ]);

            methodIl.CalculateOffsets();
            ilCursor.Index = methodIl.GetIndexByOffset(labelContinue.Offset);
            ilCursor.MatchNext(x => x.IsConditionalBranch());
            ilCursor.Index++;
            indexStart = ilCursor.Index;
            ilCursor.MatchNext(x => x.Operand is IMethodDefOrRef method && method.Name == "UpdateJounal");
        }

        methodIl.OptimizeMacros();
    }
}