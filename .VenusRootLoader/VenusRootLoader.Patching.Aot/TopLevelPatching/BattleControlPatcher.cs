using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class BattleControlPatcher : ITopLevelTypePatcher
{
    public string TypeName => "BattleControl";

    public void PatchType(GameModuleData gameModuleData, TypeDefinition type)
    {
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
        ExtractDoCommand(gameModuleData, doCommandStateMachine);

        MethodDefinition aiAttackMethod = type.Methods
            .Single(x => x.Name == "AIAttack");
        StateMachine aiAttackStateMachine = new(gameModuleData, aiAttackMethod);
        ExtractAIAttack(gameModuleData, aiAttackStateMachine);

        MethodDefinition useItemMethod = type.Methods
            .Single(x => x.Name == "UseItem");
        StateMachine useItemStateMachine = new(gameModuleData, useItemMethod);
        ExtractUseItem(gameModuleData, useItemStateMachine);
    }

    private static void PatchDoAction(
        GameModuleData gameModuleData,
        StateMachine doActionStateMachine)
    {
        SwitchArmsCoroutineExtractor doActionSwitchArmsExtractor = new(gameModuleData, doActionStateMachine);
        ExtractPlayerActions(doActionStateMachine, doActionSwitchArmsExtractor);
        ExtractEnemyActions(doActionStateMachine, doActionSwitchArmsExtractor);
    }

    private static void ExtractPlayerActions(
        StateMachine doActionStateMachine,
        SwitchArmsCoroutineExtractor doActionSwitchArmsExtractor)
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
            "PlayerActionContext",
            [
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("entity", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("actionid", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startp", true),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("startstate", false),
                doActionStateMachine.AddContextFieldFromSpeakableFieldName("targetentity", true),
            ]);

        doActionSwitchArmsExtractor.ExtractSwitchArms(
            "PlayerAction",
            playerActionSwitch,
            beforePlayerActionSwitch,
            null,
            null);
    }

    private static void ExtractEnemyActions(
        StateMachine doActionStateMachine,
        SwitchArmsCoroutineExtractor doActionSwitchArmsExtractor)
    {
        CilInstructionCollection doActionMoveNextIl =
            doActionStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(doActionMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Ldfld && ((IFieldDescriptor)x.Operand!).Name == "firststrike");
        ilCursor.MatchNext(x => x.OpCode == Ldfld && ((IFieldDescriptor)x.Operand!).Name == "onground");
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction enemyActionSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdloc() && (x.OpCode == Ldloc_1 || x.Operand is CilLocalVariable { Index: 1 }));
        CilInstruction beforeEnemyActionSwitch = ilCursor.Current();

        doActionStateMachine.AssignNewContext(
            "EnemyActionContext",
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
            "EnemyAction",
            enemyActionSwitch,
            beforeEnemyActionSwitch,
            beforeEnemyActionSwitch,
            EnemyActionPostProcessor);
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

        SwitchArmsCoroutineExtractor eventDialoguesSwitchArmsExtractor = new(gameModuleData, eventDialogueStateMachine);
        eventDialoguesSwitchArmsExtractor.ExtractSwitchArms(
            "EventDialogue",
            eventDialogueSwitch,
            beforeEventDialogueSwitch,
            null,
            null);
    }

    private static void ExtractDoCommand(GameModuleData gameModuleData, StateMachine doCommandStateMachine)
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
            "DoCommandSetupContext",
            [
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("timer", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("commandtype", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("data", true),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("internaldata", false),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("intdata", false),
                doCommandStateMachine.AddContextFieldFromSpeakableFieldName("letters", false)
            ]);

        SwitchArmsCoroutineIntoMethodsExtractor doCommandSetupSwitchArmsExtractor = new(
            gameModuleData,
            doCommandStateMachine);
        doCommandSetupSwitchArmsExtractor.ExtractSwitchArms(
            "DoCommandSetup",
            doCommandSetupSwitch,
            beforeDoCommandSetupSwitch,
            null,
            null);

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
            "DoCommandExecutionContext",
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

        SwitchArmsCoroutineExtractor doCommandSwitchArmsExtractor = new(gameModuleData, doCommandStateMachine);
        doCommandSwitchArmsExtractor.ExtractSwitchArms(
            "DoCommandExecution",
            doCommandExecutionSwitch,
            beforeDoCommandSwitch,
            null,
            null);
    }

    private static void ExtractAIAttack(GameModuleData gameModuleData, StateMachine aiAttackStateMachine)
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
            "AIAttackContext",
            [
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("targetid", false),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("dammod", true),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("nodamage", false),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("sp", true),
                aiAttackStateMachine.AddContextFieldFromSpeakableFieldName("aid", true)
            ]);

        SwitchArmsCoroutineExtractor aiAttackSwitchArmsExtractor = new(gameModuleData, aiAttackStateMachine);
        aiAttackSwitchArmsExtractor.ExtractSwitchArms(
            "AIAttack",
            newSwitchInstruction,
            beforeAiAttackSwitch,
            null,
            null);
    }

    private static void ExtractUseItem(GameModuleData gameModuleData, StateMachine useItemStateMachine)
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
            "UseItemBattleEffectContext",
            [
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("id", true),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("san", false),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("itemuse", true),
                useItemStateMachine.AddContextFieldFromSpeakableFieldName("i", true),
                useItemStateMachine.AddContextFieldFromLocalIndex(4, true),
                useItemStateMachine.AddContextFieldFromLocalIndex(5, false)
            ]);

        SwitchArmsCoroutineIntoMethodsExtractor useItemSetupSwitchArmsExtractor = new(
            gameModuleData,
            useItemStateMachine);
        useItemSetupSwitchArmsExtractor.ExtractSwitchArms(
            "UseItemBattleEffect",
            useItemSwitch,
            beforeUseItemSetupSwitch,
            null,
            null);
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