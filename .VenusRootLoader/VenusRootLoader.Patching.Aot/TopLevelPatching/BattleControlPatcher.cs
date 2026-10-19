using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.Factories;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors.SwitchArmsExtractors;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class BattleControlPatcher : TopLevelTypePatcher
{
    public override string TypeName => "BattleControl";

    public override void PatchType(GameModuleData gameModuleData, TypeDefinition type)
    {
        MethodDefinition startBattleMethod = type.Methods
            .Single(x => x.Name == "StartBattle");
        StateMachine startBattleStateMachine = new(gameModuleData, startBattleMethod);
        Dictionary<int, string> animIdsEnum = BuildEnumValueToNameMapping(gameModuleData, "AnimIDs");
        PatchStartBattle(gameModuleData, startBattleStateMachine, animIdsEnum);

        MethodDefinition doActionMethod = type.Methods
            .Single(x => x.Name == "DoAction");
        StateMachine doActionStateMachine = new(gameModuleData, doActionMethod);
        PatchDoAction(gameModuleData, doActionStateMachine);

        MethodDefinition eventDialogueMethod = type.Methods
            .Single(x => x.Name == "EventDialogue");
        StateMachine eventDialogueStateMachine = new(gameModuleData, eventDialogueMethod);
        ExtractEventDialogues(gameModuleData, eventDialogueStateMachine);

        MethodDefinition doCommandMethod = type.Methods
            .Single(x => x.Name == "DoCommand");
        StateMachine doCommandStateMachine = new(gameModuleData, doCommandMethod);
        Dictionary<int, string> actionCommandsEnum = BuildEnumValueToNameMapping(gameModuleData, "ActionCommands");
        ExtractDoCommand(gameModuleData, doCommandStateMachine, actionCommandsEnum);

        MethodDefinition aiAttackMethod = type.Methods
            .Single(x => x.Name == "AIAttack");
        StateMachine aiAttackStateMachine = new(gameModuleData, aiAttackMethod);
        ExtractAIAttack(gameModuleData, aiAttackStateMachine, animIdsEnum);

        MethodDefinition useItemMethod = type.Methods
            .Single(x => x.Name == "UseItem");
        StateMachine useItemStateMachine = new(gameModuleData, useItemMethod);
        Dictionary<int, string> itemsEnum = BuildEnumValueToNameMapping(gameModuleData, "ItemUsage");
        ExtractUseItem(gameModuleData, useItemStateMachine, itemsEnum);
    }

    private static void PatchStartBattle(
        GameModuleData gameModuleData,
        StateMachine startBattleStateMachine,
        Dictionary<int, string> animIdsEnum)
    {
        CilInstructionCollection il = startBattleStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(il);

        ilCursor.MatchNext(x => x.OpCode == Stfld && x.Operand is FieldDefinition field && field.Name == "aiparty");
        ilCursor.Index++;
        int indexStart = ilCursor.Index;

        ilCursor.MatchNext(x => x.OpCode == Ldfld && x.Operand is FieldDefinition field && field.Name == "aiparty");
        ilCursor.Index--;
        CilInstruction instructionStop = ilCursor.Current();

        ilCursor.Index = indexStart;
        while (ilCursor.Current().Offset < instructionStop.Offset)
        {
            ilCursor.MatchNext(x => x.OpCode == Callvirt
                                    && x.Operand is MethodDefinition method
                                    && method.Name == "AddAI");
            int animId = il[ilCursor.Index - 2].GetLdcI4Constant();
            ilCursor.MatchPrevious(x => x.IsConditionalBranch());
            ICilLabel labelNext = (ICilLabel)ilCursor.Current().Operand!;
            ilCursor.Index++;
            il.Insert(ilCursor.Index, Br, il[ilCursor.Index].CreateLabel());
            int indexEnd = ilCursor.Index;

            CilInstruction instructionStart = il[indexStart];
            CilInstruction instructionEnd = il[indexEnd];
            MoveNextToMethodLogicExtractor extractor = new(
                startBattleStateMachine,
                new InnerMethodFromCoroutineFactory(),
                gameModuleData.Module.CorLibTypeFactory.Boolean,
                gameModuleData,
                $"StartBattle_HelperPredicate_{animIdsEnum[animId + 1]}",
                instructionStart,
                instructionEnd);
            il.CalculateOffsets();
            extractor.ExtractIl(
                [],
                new Dictionary<int, List<CilInstruction>>
                {
                    [labelNext.Offset] = [new(Ldc_I4_0)],
                    [il[ilCursor.Index + 1].Offset] = [new(Ldc_I4_1)]
                },
                null,
                out _);

            il.ReplaceRange(
                indexStart,
                indexEnd,
                [
                    new(Call, extractor.InnerLogic.ReceivingMethod),
                    new(Brfalse, labelNext)
                ]);

            ilCursor.Index = il.GetIndexByOffset(labelNext.Offset);
            indexStart = ilCursor.Index;
        }
    }

    private static void PatchDoAction(
        GameModuleData gameModuleData,
        StateMachine doActionStateMachine)
    {
        SwitchArmsStateMachineExtractor doActionSwitchArmsExtractor = new(gameModuleData, doActionStateMachine);
        Dictionary<int, string> skillsEnum = BuildEnumValueToNameMapping(gameModuleData, "Skills");
        ExtractPlayerActions(doActionStateMachine, doActionSwitchArmsExtractor, skillsEnum);
        Dictionary<int, string> enemiesEnum = BuildEnumValueToNameMapping(gameModuleData, "Enemies");
        ExtractEnemyActions(doActionStateMachine, doActionSwitchArmsExtractor, enemiesEnum);
    }

    private static void ExtractPlayerActions(
        StateMachine doActionStateMachine,
        SwitchArmsStateMachineExtractor doActionSwitchArmsExtractor,
        Dictionary<int, string> skillsEnum)
    {
        CilInstructionCollection doActionMoveNextIl =
            doActionStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(doActionMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Ldstr && (string)x.Operand! == "Player");
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction playerActionSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x =>
            x.IsLdarg() && (x.OpCode == Ldarg_0 || x.Operand is Parameter { MethodSignatureIndex: 0 }));
        CilInstruction beforePlayerActionSwitch = ilCursor.Current();

        doActionStateMachine.AssignNewContext(
            "PlayerAction",
            [
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("entity", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("actionid", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startp", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startstate", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("targetentity", true),
            ]);

        doActionSwitchArmsExtractor.ExtractSwitchArms(
            playerActionSwitch,
            beforePlayerActionSwitch,
            null,
            null,
            skillsEnum,
            "PlayerAction");
    }

    private static void ExtractEnemyActions(
        StateMachine doActionStateMachine,
        SwitchArmsStateMachineExtractor doActionSwitchArmsExtractor,
        Dictionary<int, string> enemiesEnum)
    {
        CilInstructionCollection doActionMoveNextIl =
            doActionStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        doActionMoveNextIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(doActionMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Ldfld && ((IFieldDescriptor)x.Operand!).Name == "firststrike");
        ilCursor.MatchNext(x => x.OpCode == Ldfld && ((IFieldDescriptor)x.Operand!).Name == "onground");
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction enemyActionSwitch = ilCursor.Current();
        int indexEnemyActionSwitch = ilCursor.Index;
        ilCursor.Index++;
        ICilLabel switchEndLabel = (ICilLabel)ilCursor.Current().Operand!;
        ilCursor.Index = doActionMoveNextIl.GetIndexByOffset(switchEndLabel.Offset);
        ilCursor.MatchNext(x => x.IsLdloc());
        CilInstruction additionalSwitchEnd = ilCursor.Current();
        ilCursor.Index = indexEnemyActionSwitch;

        ilCursor.MatchPrevious(x => x.IsLdloc() && (x.OpCode == Ldloc_1 || x.Operand is CilLocalVariable { Index: 1 }));
        CilInstruction beforeEnemyActionSwitch = ilCursor.Current();

        doActionStateMachine.AssignNewContext(
            "EnemyAction",
            [
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("entity", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("actionid", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("randomposafter", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("fled", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("nocharm", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startp", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startstate", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("heavystrike", true, "hardmode")
            ]);

        doActionSwitchArmsExtractor.ExtractSwitchArms(
            enemyActionSwitch,
            beforeEnemyActionSwitch,
            beforeEnemyActionSwitch,
            EnemyActionPostProcessor,
            enemiesEnum,
            "EnemyAction",
            [additionalSwitchEnd]);
    }

    private static void ExtractEventDialogues(GameModuleData gameModuleData, StateMachine eventDialogueStateMachine)
    {
        CilInstructionCollection eventDialogueMoveNextIl =
            eventDialogueStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(eventDialogueMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction eventDialogueSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x =>
            x.IsLdarg() && (x.OpCode == Ldarg_0 || x.Operand is Parameter { MethodSignatureIndex: 0 }));
        CilInstruction beforeEventDialogueSwitch = ilCursor.Current();

        SwitchArmsStateMachineExtractor eventDialoguesSwitchArmsExtractor = new(
            gameModuleData,
            eventDialogueStateMachine);
        eventDialoguesSwitchArmsExtractor.ExtractSwitchArms(
            eventDialogueSwitch,
            beforeEventDialogueSwitch,
            null,
            null,
            []);
    }

    private static void ExtractDoCommand(
        GameModuleData gameModuleData,
        StateMachine doCommandStateMachine,
        Dictionary<int, string> actionCommandsEnum)
    {
        CilInstructionCollection doCommandMoveNextIl = doCommandStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(doCommandMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction doCommandSetupSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdloc() && (x.OpCode == Ldloc_2 || x.Operand is CilLocalVariable { Index: 2 }));
        CilInstruction beforeDoCommandSetupSwitch = ilCursor.Current();

        doCommandStateMachine.AssignNewContext(
            "Setup",
            [
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("timer", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("commandtype", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("data", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("internaldata", false),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("intdata", false),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("letters", false)
            ]);

        SwitchArmsStateMachineIntoMethodsExtractor doCommandSetupSwitchArmsExtractor = new(
            gameModuleData,
            doCommandStateMachine);
        doCommandSetupSwitchArmsExtractor.ExtractSwitchArms(
            doCommandSetupSwitch,
            beforeDoCommandSetupSwitch,
            null,
            null,
            actionCommandsEnum,
            "Setup");

        ilCursor.Index = 0;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction doCommandExecutionSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdloc() && (x.OpCode == Ldloc_2 || x.Operand is CilLocalVariable { Index: 2 }));
        CilInstruction beforeDoCommandSwitch = ilCursor.Current();

        doCommandStateMachine.AssignNewContext(
            "Execution",
            [
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("timer", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("commandtype", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("data", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("internaldata", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("initialtimer", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("infinite", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("intdata", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("letters", true)
            ]);

        SwitchArmsStateMachineExtractor doCommandSwitchArmsExtractor = new(gameModuleData, doCommandStateMachine);
        doCommandSwitchArmsExtractor.ExtractSwitchArms(
            doCommandExecutionSwitch,
            beforeDoCommandSwitch,
            null,
            null,
            actionCommandsEnum,
            "Execution");
    }

    private static void ExtractAIAttack(
        GameModuleData gameModuleData,
        StateMachine aiAttackStateMachine,
        Dictionary<int, string> animIdsEnum)
    {
        CilInstructionCollection aiAttackMoveNextIl =
            aiAttackStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(aiAttackMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.MatchPrevious(x =>
            x.OpCode == Ldfld && x.Operand is FieldDefinition fieldLoaded && fieldLoaded.Name!.Contains("aid"));
        ilCursor.MatchNext(x => x.IsLdloc());

        CilInstruction beforeAiAttackSwitch = ilCursor.Current();
        ilCursor.Index++;
        int indexSwitchStart = ilCursor.Index;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index += 2;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        CilInstructionLabel switchEndLabel = (CilInstructionLabel)ilCursor.Current().Operand!;
        ilCursor.Index--;
        int indexSwitchEnd = ilCursor.Index;

        ilCursor.Index = indexSwitchStart;

        Dictionary<int, CilInstructionLabel> usedSwitchCases = [];
        while (ilCursor.Index <= indexSwitchEnd)
        {
            ilCursor.MatchNext(x => x.IsLdcI4());
            int ldcConstant = ilCursor.Current().GetLdcI4Constant();
            ilCursor.Index++;
            CilInstruction nextInstruction = ilCursor.Current();
            if (nextInstruction.OpCode == Beq)
            {
                usedSwitchCases.Add(ldcConstant, (CilInstructionLabel)nextInstruction.Operand!);
            }
            else if (nextInstruction.OpCode == Sub)
            {
                ilCursor.Index++;
                List<ICilLabel> labels = (List<ICilLabel>)ilCursor.Current().Operand!;
                for (int i = 0; i < labels.Count; i++)
                {
                    CilInstructionLabel instructionLabel = (CilInstructionLabel)labels[i];
                    if (Equals(instructionLabel.Instruction, switchEndLabel.Instruction))
                        continue;
                    usedSwitchCases.Add(i + ldcConstant, instructionLabel);
                }
            }

            ilCursor.Index++;
        }

        List<ICilLabel> newLabels = [];
        for (int i = 0; i < 407; i++)
            newLabels.Add(usedSwitchCases.GetValueOrDefault(i, switchEndLabel));

        aiAttackMoveNextIl.ReplaceRange(indexSwitchStart, indexSwitchEnd, [new(Switch, newLabels)]);
        ilCursor.Index = indexSwitchStart;
        CilInstruction newSwitchInstruction = ilCursor.Current();

        aiAttackStateMachine.AssignNewContext(
            "",
            [
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("targetid", false),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("dammod", true),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("nodamage", false),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("sp", true),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("aid", true)
            ]);

        SwitchArmsStateMachineExtractor aiAttackSwitchArmsExtractor = new(gameModuleData, aiAttackStateMachine);
        aiAttackSwitchArmsExtractor.ExtractSwitchArms(
            newSwitchInstruction,
            beforeAiAttackSwitch,
            null,
            null,
            animIdsEnum);
    }

    private static void ExtractUseItem(
        GameModuleData gameModuleData,
        StateMachine useItemStateMachine,
        Dictionary<int, string> itemsEnum)
    {
        CilInstructionCollection useItemMoveNextIl = useItemStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(useItemMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction useItemSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdloc());
        CilInstruction beforeUseItemSetupSwitch = ilCursor.Current();

        useItemStateMachine.AssignNewContext(
            "BattleEffect",
            [
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("id", true),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("san", false),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("itemuse", true),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("i", true),
                useItemStateMachine.AddContextFieldFromLocalIndex(4, true),
                useItemStateMachine.AddContextFieldFromLocalIndex(5, false)
            ]);

        SwitchArmsStateMachineIntoMethodsExtractor useItemSetupSwitchArmsExtractor = new(
            gameModuleData,
            useItemStateMachine);
        useItemSetupSwitchArmsExtractor.ExtractSwitchArms(
            useItemSwitch,
            beforeUseItemSetupSwitch,
            null,
            null,
            itemsEnum,
            "BattleEffect");
    }

    private static void EnemyActionPostProcessor(
        int switchLabelIndex,
        StateMachine stateMachine,
        GameModuleData gameModuleData)
    {
        if (switchLabelIndex != 1)
            return;

        PatchMushroomCField(stateMachine, gameModuleData);
    }

    // This is needed to avoid having the "c" field of DoAction to be part of the context unnecessarily. Only the Mushroom
    // enemy reads from it before writing it so we can just patch this one specifically to have the same value it would have
    // had before the enemy actions split.
    private static void PatchMushroomCField(StateMachine stateMachine, GameModuleData gameModuleData)
    {
        ModuleDefinition module = stateMachine.StateMachineType.DeclaringModule!;
        FieldDefinition cField =
            stateMachine.StateMachineType.Fields.Single(x => x.Name!.Value.Contains("<c>"));

        AssemblyReference unityCoreModule = gameModuleData.UnityCoreModuleReference;
        IMethodDescriptor randomRangeIntInt = unityCoreModule
            .CreateTypeReference("UnityEngine", "Random")
            .CreateMethodReference(
                "Range",
                MethodSignature.CreateStatic(
                    module.CorLibTypeFactory.Int32,
                    [module.CorLibTypeFactory.Int32, module.CorLibTypeFactory.Int32]));

        CilMethodBody moveNextBody = stateMachine.MoveNextMethod.CilMethodBody!;
        AsmResolverIlCursor cursorMoveNext = new(moveNextBody.Instructions);
        cursorMoveNext.MatchNext(x => x.OpCode == Ldfld && x.Operand == cField);
        cursorMoveNext.MatchPrevious(x => x.IsLdarg());

        moveNextBody.Instructions.InsertRange(
            cursorMoveNext.Index,
            [
                new(Ldarg_0),
                CilInstruction.CreateLdcI4(0),
                CilInstruction.CreateLdcI4(100),
                new(Call, randomRangeIntInt),
                new(Conv_R4),
                new(Stfld, cField)
            ]);
    }
}