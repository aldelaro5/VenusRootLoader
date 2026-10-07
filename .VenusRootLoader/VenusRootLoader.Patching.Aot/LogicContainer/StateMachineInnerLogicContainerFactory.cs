using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.StateMachineUtils;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

public sealed class StateMachineInnerLogicContainerFactory : IInnerLogicContainerFactory<StateMachine, StateMachine>
{
    public StateMachine Create(
        StateMachine outerContainer,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string name)
    {
        StateMachine stateMachine = InnerStateMachineCreator.CreateAndAddInnerStateMachineType(
            outerContainer,
            gameModuleData,
            parameters,
            name);
        CilLocalVariable localState = new(stateMachine.StateField.Signature!.FieldType);
        CilLocalVariable? localThis = stateMachine.ThisField is not null
            ? new(stateMachine.ThisField.Signature!.FieldType)
            : null;
        CreateMoveNextBodyWithStateSwitchSetupInstructions(
            stateMachine,
            localState,
            localThis);
        return stateMachine;
    }

    private static void CreateMoveNextBodyWithStateSwitchSetupInstructions(
        StateMachine innerStateMachine,
        CilLocalVariable localState,
        CilLocalVariable? localThis)
    {
        CilInstruction stateSwitchInstruction = new(Switch);
        innerStateMachine.MoveNextMethod.CilMethodBody = new()
        {
            InitializeLocals = true,
            LocalVariables = { localState },
            Instructions =
            {
                Ldarg_0,
                { Ldfld, innerStateMachine.StateField },
                { Stloc, localState }
            }
        };

        if (localThis is not null)
            innerStateMachine.MoveNextMethod.CilMethodBody.LocalVariables.Add(localThis);

        if (localThis is not null)
        {
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldarg_0);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldfld, innerStateMachine.ThisField!);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Stloc, localThis);
        }

        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldloc, localState);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(stateSwitchInstruction);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldc_I4_0);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ret);
    }
}