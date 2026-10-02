using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot;

public static class Program
{
    public static int Main(string[] args)
    {
        AssemblyDefinition assembly = AssemblyDefinition.FromFile(args[0]);

        LocalNetStandardReferenceImporter referenceImporter = new(assembly.ManifestModule!);

        TypeDefinition battleControlType = assembly.ManifestModule!
            .GetAllTypes()
            .Single(x => x.Name == "BattleControl");
        MethodDefinition doActionMethod = battleControlType.Methods
            .Single(x => x.Name == "DoAction");
        StateMachine doActionStateMachine = StateMachine.CreateFromEnumeratorMethod(doActionMethod);

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
            doActionStateMachine.GetContextFieldFromSpeakableName("entity"),
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
            referenceImporter,
            doActionStateMachine,
            stateMachineContextInfo,
            enemyActionSwitch,
            beforeEnemyActionSwitch,
            beforeEnemyActionSwitch,
            StateMachinePostProcessor);

        string directory = Path.GetDirectoryName(args[1])!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        assembly.Write(args[1]);

        // string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "IL");
        // DisassemblerTests.DisassembleTypes(args[1], new DirectoryInfo(outputPath));
        return 0;
    }

    private static void StateMachinePostProcessor(int switchLabelIndex, StateMachine stateMachine)
    {
        if (switchLabelIndex != 1)
            return;

        PatchMushroomCField(stateMachine);
    }

    // This is needed to avoid having the "c" field of DoAction to be part of the context unnecessarily. Only the Mushroom
    // enemy reads from it before writing it so we can just patch this one specifically to have the same value it would have
    // had before the enemy actions split.
    private static void PatchMushroomCField(StateMachine stateMachine)
    {
        ModuleDefinition module = stateMachine.StateMachineType.DeclaringModule!;
        FieldDefinition cField =
            stateMachine.StateMachineType.Fields.Single(x => x.Name!.Value.Contains("<c>"));

        AssemblyReference unityCoreModule =
            module.AssemblyReferences.Single(x => x.Name == "UnityEngine.CoreModule");
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