using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction;

public sealed class MethodLogicExtractor : LogicExtractor<MethodLogicContainer, MethodLogicContainer>
{
    public MethodLogicExtractor(
        MethodLogicContainer outerLogic,
        IInnerLogicContainerFactory<MethodLogicContainer, MethodLogicContainer> innerLogicContainerFactory,
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
        if (!OuterLogic.ReceivingMethod.IsStatic && instructionParameter.MethodSignatureIndex == 0)
            return null;

        if (OuterLogic.ContextInfo is null || !OuterLogic.ContextInfo.ContextFieldsMapping.TryGetValue(
                instructionParameter.MethodSignatureIndex,
                out FieldDefinition? fieldInContext))
            return null;

        if (instruction.IsLdarg())
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return new(Ldfld, fieldInContext);
        }

        if (instruction.OpCode == Ldarga || instruction.OpCode == Ldarga_S)
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return new(Ldflda, fieldInContext);
        }

        if (!instruction.IsStarg())
            return null;

        int indexLastNop = GetInstructionIndexForLoadBeforeStore();
        InnerBody.Instructions.Insert(indexLastNop, InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
        instruction.ReplaceWith(Stfld, fieldInContext);

        return null;
    }

    protected override CilInstruction? ProcessLocalOperationInstruction(
        CilInstruction instruction,
        CilLocalVariable local)
    {
        if (OuterLogic.ContextInfo is null || !OuterLogic.ContextInfo.ContextFieldsMapping.TryGetValue(
                local,
                out FieldDefinition? fieldInContext))
        {
            return base.ProcessLocalOperationInstruction(instruction, local);
        }

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

        if (!instruction.IsStloc())
            return base.ProcessLocalOperationInstruction(instruction, local);

        int indexLastNop = GetInstructionIndexForLoadBeforeStore();
        InnerBody.Instructions.Insert(indexLastNop, InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
        instruction.ReplaceWith(Stfld, fieldInContext);
        return null;
    }

    protected override void ProcessFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        return;
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