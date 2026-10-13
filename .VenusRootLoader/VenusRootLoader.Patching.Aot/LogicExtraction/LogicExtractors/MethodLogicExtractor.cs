using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using Echo.Ast;
using Echo.Ast.Construction;
using Echo.ControlFlow;
using Echo.Graphing;
using Echo.Platforms.AsmResolver;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.Factories;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors;

/// <summary>
/// An extractor that can extract logic from an outer <see cref="MethodLogicContainer"/> to an inner <see cref="MethodLogicContainer"/>.
/// </summary>
public sealed class MethodLogicExtractor : LogicExtractor<MethodLogicContainer, MethodLogicContainer>
{
    private ControlFlowGraph<Statement<CilInstruction>> _outerAst = null!;
    private CompilationUnit<CilInstruction> _compilationUnit = null!;

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

    protected override void PreProcessOuterClone(CilMethodBody outerCloneBody)
    {
        outerCloneBody.Instructions.CalculateOffsets();
        _outerAst = outerCloneBody
            .ConstructStaticFlowGraph()
            .Lift(new CilPurityClassifier());
        _compilationUnit = _outerAst.ToCompilationUnit();
    }

    protected override List<CilInstruction> ProcessArgumentOperationInstruction(
        CilInstruction instruction,
        Parameter instructionParameter)
    {
        // arg 0 for instance method is always the "this" so we don't need to do anything for this case.
        if (!OuterLogic.ReceivingMethod.IsStatic && instructionParameter.MethodSignatureIndex == 0)
            return EmptyIl;

        if (OuterLogic.Context is null || !OuterLogic.Context.ContextFieldsMapping.TryGetValue(
                instructionParameter.MethodSignatureIndex,
                out FieldDefinition? fieldInContext))
            return EmptyIl;

        if (instruction.IsLdarg())
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return [new(Ldfld, fieldInContext)];
        }

        if (instruction.OpCode == Ldarga || instruction.OpCode == Ldarga_S)
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return [new(Ldflda, fieldInContext)];
        }

        if (!instruction.IsStarg())
            return EmptyIl;

        ProcessContextFieldStore(instruction, fieldInContext);
        return EmptyIl;
    }

    protected override List<CilInstruction> ProcessLocalOperationInstruction(
        CilInstruction instruction,
        CilLocalVariable localVariable)
    {
        if (OuterLogic.Context is null || !OuterLogic.Context.ContextFieldsMapping.TryGetValue(
                localVariable,
                out FieldDefinition? fieldInContext))
        {
            return base.ProcessLocalOperationInstruction(instruction, localVariable);
        }

        if (instruction.IsLdloc())
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return [new(Ldfld, fieldInContext)];
        }

        if (instruction.OpCode == Ldloca || instruction.OpCode == Ldloca_S)
        {
            instruction.ReplaceWith(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1);
            return [new(Ldflda, fieldInContext)];
        }

        if (!instruction.IsStloc())
            return base.ProcessLocalOperationInstruction(instruction, localVariable);

        ProcessContextFieldStore(instruction, fieldInContext);
        return EmptyIl;
    }

    private void ProcessContextFieldStore(CilInstruction instruction, FieldDefinition fieldInContext)
    {
        ControlFlowNode<Statement<CilInstruction>> currentNode =
            _outerAst.Nodes.Last(x => x.Offset <= instruction.Offset);
        Statement<CilInstruction> statement = currentNode.Contents.Instructions
            .Last(x => x.OriginalRange is not null
                       && x.OriginalRange.Value.Start <= instruction.Offset
                       && x.OriginalRange.Value.End >= instruction.Offset);

        int foundOffset = GetLdArgIndex(statement);
        // The only link we have with the original outer method is the offsets so we can't recalculate them, but we can
        // only do our ldarg by getting the instruction index so we have to linear search it. Doing it backward since
        // it's likely going to be near the end by this point.
        int foundIndex = -1;
        for (int i = InnerBody.Instructions.Count - 1; i >= 0; i--)
        {
            if (InnerBody.Instructions[i].Offset != foundOffset)
                continue;

            foundIndex = i;
            break;
        }

        CilInstruction originalInstruction = InnerBody.Instructions[foundIndex];
        InnerBody.Instructions.ReplaceRange(
            foundIndex,
            foundIndex,
            [
                new(InnerLogic.ReceivingMethod.IsStatic ? Ldarg_0 : Ldarg_1),
                new(originalInstruction.OpCode, originalInstruction.Operand)
            ]);
        instruction.ReplaceWith(Stfld, fieldInContext);
    }

    private int GetLdArgIndex(Statement<CilInstruction> statement)
    {
        // A phi statement means this is 2 branch merging so we need to find the common ancestor between the 2 branches
        // and assume the last statement to be the place we want to resume searching.
        if (statement is PhiStatement<CilInstruction> phiStatement)
        {
            IEnumerable<Statement<CilInstruction>> phiWrites = phiStatement.Sources
                .SelectMany(x => x.Variable.GetIsWrittenBy(_compilationUnit));
            List<ControlFlowNode<Statement<CilInstruction>>> cfgNodes = phiWrites
                .Select(x => _outerAst.GetNodeByOffset(x.Parent!.OriginalRange!.Value.Start))
                .Distinct()
                .ToList();

            HashSet<ControlFlowNode<Statement<CilInstruction>>> ancestors = [];
            while (true)
            {
                List<ControlFlowNode<Statement<CilInstruction>>> predecessors = cfgNodes
                    .SelectMany(x => x.GetPredecessors())
                    .Distinct()
                    .ToList();
                if (predecessors.Count == 1)
                    return GetLdArgIndex(predecessors[0].Contents.Instructions.Last());

                // Making sure we find a common ancestor even if both branches have a different amount of nodes leading
                // to them.
                foreach (ControlFlowNode<Statement<CilInstruction>> predecessor in predecessors)
                {
                    if (!ancestors.Add(predecessor))
                        return GetLdArgIndex(predecessor.Contents.Instructions.Last());
                }

                cfgNodes = predecessors;
            }
        }

        // A variable expression means that we can't insert our ldarg because it's in the middle of computing it through intermediary
        // stack variables. We need to make sure every leaf don't have them to accept the statement.
        List<VariableExpression<CilInstruction>> varExpressions = GetVarExpressionLeaves(statement);
        if (varExpressions.Count == 0)
            return (int)statement.OriginalRange!.Value.Start;

        IEnumerable<Statement<CilInstruction>> varWrites =
            varExpressions.SelectMany(x => x.Variable.GetIsWrittenBy(_compilationUnit));

        // If there's multiple choice, always pick the earliest one.
        return varWrites.Select(GetLdArgIndex).Min();
    }

    private static List<VariableExpression<CilInstruction>> GetVarExpressionLeaves(TreeNodeBase statement)
    {
        List<TreeNodeBase> child = statement.GetChildren().ToList();
        if (child.Count == 0)
        {
            return statement is VariableExpression<CilInstruction> varExpression
                ? [varExpression]
                : [];
        }

        return child.SelectMany(GetVarExpressionLeaves).ToList();
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