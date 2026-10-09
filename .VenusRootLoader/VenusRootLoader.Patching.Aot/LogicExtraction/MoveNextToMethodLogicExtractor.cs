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

    protected override CilInstruction? ProcessArgumentOperationInstruction(
        CilInstruction instruction,
        Parameter instructionParameter)
    {
        if (instructionParameter.MethodSignatureIndex == 0)
            instruction.ReplaceWithNop();
        return null;
    }

    protected override CilInstruction? ProcessLocalOperationInstruction(
        CilInstruction instruction,
        CilLocalVariable local)
    {
        if (OuterLogic.ContextInfo is not null && OuterLogic.ContextInfo.ContextFieldsMapping.TryGetValue(
                local,
                out FieldDefinition? fieldInContext))
        {
            if (instruction.IsLdloc())
            {
                instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
                return new(Ldfld, fieldInContext);
            }

            if (instruction.OpCode == Ldloca || instruction.OpCode == Ldloca_S)
            {
                instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
                return new(Ldflda, fieldInContext);
            }

            if (instruction.IsStloc())
            {
                int indexLastNop = GetInstructionIndexForLoadBeforeStore();
                InnerBody.Instructions.Insert(indexLastNop, InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
                instruction.ReplaceWith(Stfld, fieldInContext);
                return null;
            }
        }

        if (OuterLogic.EnumeratorMethod.IsStatic || local.Index != 1)
            return base.ProcessLocalOperationInstruction(instruction, local);

        if (instruction.IsLdloc())
            instruction.ReplaceWith(Ldarg_0);
        else if (instruction.OpCode == Ldloca || instruction.OpCode == Ldloca_S)
            instruction.ReplaceWith(Ldarga, (byte)0);
        else if (instruction.IsStloc())
            instruction.ReplaceWith(Starg, (byte)0);

        return null;
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
        if (instruction.OpCode == Ldfld || instruction.OpCode == Ldflda)
        {
            InnerBody.Instructions.Add(Ldarg_1);
            instruction.Operand = fieldInContext;
            return;
        }

        int indexLastNop = GetInstructionIndexForLoadBeforeStore();
        InnerBody.Instructions.Insert(indexLastNop, Ldarg_1);
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
        return MethodLogicContainer.GetTransferToOtherMethodIl(
            otherContainer,
            InnerLogic.ReceivingMethod.Parameters.ToList());
    }

    protected override void PostProcessExtraction()
    {
        if (InnerBody.Instructions[^1].OpCode != Ret)
            InnerBody.Instructions.Add(Ret);
    }
}