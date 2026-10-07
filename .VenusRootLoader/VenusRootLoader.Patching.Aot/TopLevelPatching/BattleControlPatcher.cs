using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.StateMachineUtils;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class BattleControlPatcher : ITopLevelTypePatcher
{
    public string TypeName => "BattleControl";

    public void PatchType(GameModuleData gameModuleData, TypeDefinition type)
    {
        MethodDefinition doActionMethod = type.Methods
            .Single(x => x.Name == "DoAction");
        StateMachine doActionStateMachine = StateMachine.CreateFromEnumeratorMethod(gameModuleData, doActionMethod);

        ExtractPlayerActions(gameModuleData, doActionStateMachine);
        ExtractEnemyActions(gameModuleData, doActionStateMachine);

        MethodDefinition eventDialogueMethod = type.Methods
            .Single(x => x.Name == "EventDialogue");
        StateMachine eventDialogueStateMachine =
            StateMachine.CreateFromEnumeratorMethod(gameModuleData, eventDialogueMethod);

        ExtractEventDialogues(gameModuleData, eventDialogueStateMachine);

        MethodDefinition doCommandMethod = type.Methods
            .Single(x => x.Name == "DoCommand");
        StateMachine doCommandStateMachine =
            StateMachine.CreateFromEnumeratorMethod(gameModuleData, doCommandMethod);

        ExtractDoCommand(gameModuleData, doCommandStateMachine);
    }

    private static void ExtractPlayerActions(
        GameModuleData gameModuleData,
        StateMachine doActionStateMachine)
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

        List<StateMachineContextField> doActionContextFields =
        [
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("entity"),
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("actionid"),
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("startp"),
            doActionStateMachine.GetContextFieldFromSpeakableName("startstate"),
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("targetentity")
        ];

        StateMachineContextInfo stateMachineContextInfo = new()
        {
            TypeName = "PlayerActionContext",
            Fields = doActionContextFields,
        };

        SwitchArmsCoroutineExtractor.ExtractSwitchArmsToStateMachines(
            gameModuleData,
            doActionStateMachine,
            "PlayerAction",
            stateMachineContextInfo,
            playerActionSwitch,
            beforePlayerActionSwitch,
            null,
            null);
    }

    private static void ExtractEnemyActions(
        GameModuleData gameModuleData,
        StateMachine doActionStateMachine)
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

        List<StateMachineContextField> doActionContextFields =
        [
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("entity"),
            doActionStateMachine.GetContextFieldFromSpeakableName("actionid"),
            doActionStateMachine.GetContextFieldFromSpeakableName("randomposafter"),
            doActionStateMachine.GetContextFieldFromSpeakableName("fled"),
            doActionStateMachine.GetContextFieldFromSpeakableName("nocharm"),
            doActionStateMachine.GetContextFieldFromSpeakableName("startp"),
            doActionStateMachine.GetContextFieldFromSpeakableName("startstate"),
            doActionStateMachine.GetReadOnlyContextFieldFromSpeakableName("heavystrike", "hardmode")
        ];

        StateMachineContextInfo stateMachineContextInfo = new()
        {
            TypeName = "EnemyActionContext",
            Fields = doActionContextFields,
        };

        SwitchArmsCoroutineExtractor.ExtractSwitchArmsToStateMachines(
            gameModuleData,
            doActionStateMachine,
            "EnemyAction",
            stateMachineContextInfo,
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

        SwitchArmsCoroutineExtractor.ExtractSwitchArmsToStateMachines(
            gameModuleData,
            eventDialogueStateMachine,
            "EventDialogue",
            null,
            eventDialogueSwitch,
            beforeEventDialogueSwitch,
            null,
            null);
    }

    private static void ExtractDoCommand(GameModuleData gameModuleData, StateMachine doCommandStateMachine)
    {
        CilInstructionCollection doCommandMoveNextIl =
            doCommandStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(doCommandMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction doCommandExecutionSwitch = ilCursor.Current();
        ilCursor.MatchPrevious(x => x.IsLdloc() && (x.OpCode == Ldloc_2 || x.Operand is CilLocalVariable { Index: 2 }));
        CilInstruction beforeDoCommandSwitch = ilCursor.Current();

        List<StateMachineContextField> doCommandContextFields =
        [
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("timer"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("commandtype"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("data"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("internaldata"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("initialtimer"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("infinite"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("intdata"),
            doCommandStateMachine.GetReadOnlyContextFieldFromSpeakableName("letters")
        ];

        StateMachineContextInfo stateMachineContextInfo = new()
        {
            TypeName = "DoCommandExecutionContext",
            Fields = doCommandContextFields,
        };

        SwitchArmsCoroutineExtractor.ExtractSwitchArmsToStateMachines(
            gameModuleData,
            doCommandStateMachine,
            "DoCommandExecution",
            stateMachineContextInfo,
            doCommandSetupSwitch,
            beforeDoCommandSwitch,
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