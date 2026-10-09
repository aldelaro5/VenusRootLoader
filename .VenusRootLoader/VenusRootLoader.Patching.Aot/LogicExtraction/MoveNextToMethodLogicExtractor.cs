using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

public sealed class MoveNextToMethodLogicExtractor : LogicExtractor<StateMachine, MethodLogicContainer>
{
    private readonly Dictionary<FieldDefinition, CilLocalVariable> _fieldsNonContextMapping = [];

    public MoveNextToMethodLogicExtractor(
        StateMachine outerLogic,
        IInnerLogicContainerFactory<StateMachine, MethodLogicContainer> innerLogicContainerFactory,
        GameModuleData gameModuleData,
        string methodName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
        : base(
            outerLogic,
            innerLogicContainerFactory,
            gameModuleData,
            methodName,
            innerFirstInstruction,
            innerLastInstruction)
    {
    }

    protected override void ProcessArgumentOperationInstruction(
        CilInstruction instruction,
        Parameter instructionParameter)
    {
        if (instructionParameter.MethodSignatureIndex == 0)
            instruction.ReplaceWithNop();
    }

    protected override void ProcessLocalOperationInstruction(CilInstruction instruction)
    {
        if (OuterLogic.EnumeratorMethod.IsStatic)
        {
            base.ProcessLocalOperationInstruction(instruction);
            return;
        }

        CilLocalVariable local = instruction.GetLocalVariable(OuterLogic.ReceivingMethod.CilMethodBody!.LocalVariables);
        if (local.Index != 1)
        {
            base.ProcessLocalOperationInstruction(instruction);
            return;
        }

        if (instruction.IsLdloc())
            instruction.ReplaceWith(Ldarg, (byte)0);
        else if (instruction.OpCode == Ldarga || instruction.OpCode == Ldarga_S)
            instruction.ReplaceWith(Ldarga, (byte)0);
        else if (instruction.IsStarg())
            instruction.ReplaceWith(Starg, (byte)0);
    }

    protected override void ProcessFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        if (OuterLogic.ContextInfo is not null && OuterLogic.ContextInfo.ContextFieldsMapping.TryGetValue(
                instructionField,
                out FieldDefinition? fieldInContext))
            ProcessContextFieldOperationInstruction(instruction, fieldInContext);
        else
            ProcessNonContextFieldOperationInstruction(instruction, instructionField);
    }

    private void ProcessContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition fieldInContext)
    {
        // Mapping a context field has a complication: the field belongs to the context, not to a field of the state machine.
        // Because of this, we need to insert an ldarg.1, but to figure out where, we do the assumption that the state machine
        // was placed on the stack using ldarg.0, which we NOPed earlier. We need to look for the NOP so we can insert our
        // ldfld and have the field resolve correctly.
        int indexLastNop = -1;
        for (int j = InnerBody.Instructions.Count - 1; j >= 0; j--)
        {
            if (InnerBody.Instructions[j].OpCode != Nop)
                continue;

            indexLastNop = j;
            break;
        }

        InnerBody.Instructions[indexLastNop].ReplaceWith(Ldarg_1);
        instruction.Operand = fieldInContext;
    }

    private void ProcessNonContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        if (!_fieldsNonContextMapping.TryGetValue(instructionField, out CilLocalVariable? mappedLocal))
        {
            CilLocalVariable newLocal = new(instructionField.Signature!.FieldType);
            InnerBody.LocalVariables.Add(newLocal);
            InnerBody.InitializeLocals = true;
            _fieldsNonContextMapping.Add(instructionField, newLocal);
            mappedLocal = newLocal;
        }

        if (instruction.OpCode == Ldfld)
            instruction.OpCode = Ldloc;
        else if (instruction.OpCode == Ldflda)
            instruction.OpCode = Ldloca;
        else if (instruction.OpCode == Stfld)
            instruction.OpCode = Stloc;

        instruction.Operand = mappedLocal;
    }

    protected override IList<CilInstruction> GetReturnToOuterIl(bool afterLogicTransfer) => [new(Ret)];

    protected override ICilLabel GetRestartLabel() => InnerBody.Instructions[0].CreateLabel();

    protected override IList<CilInstruction> GetLogicTransferIl(
        MethodLogicContainer otherContainer,
        CilInstruction instructionAfter)
    {
        List<FieldDefinition> fieldArguments = OuterLogic.ContextInfo?.ContextField is not null
            ? [OuterLogic.ContextInfo.ContextField]
            : [];
        return StateMachine.GetTransferToMethodIl(otherContainer, fieldArguments, instructionAfter.CreateLabel());
    }

    protected override void PostProcessExtraction()
    {
        if (InnerBody.Instructions[^1].OpCode != Ret)
            InnerBody.Instructions.Add(Ret);
    }
}