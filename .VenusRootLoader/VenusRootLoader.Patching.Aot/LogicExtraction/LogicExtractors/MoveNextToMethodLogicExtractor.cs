using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Ast;
using Echo.Ast.Construction;
using Echo.ControlFlow;
using Echo.Graphing;
using Echo.Platforms.AsmResolver;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers;
using VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.Factories;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;
using MethodLogicContainer = VenusRootLoader.Patching.Aot.LogicExtraction.LogicContainers.MethodLogicContainer;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.LogicExtractors;

/// <summary>
/// An extractor that can extract logic from an outer <see cref="StateMachine"/> to an inner <see cref="MethodLogicContainer"/>.
/// This only works if the outer logic never performs a yield return.
/// </summary>
public sealed class MoveNextToMethodLogicExtractor : LogicExtractor<StateMachine, MethodLogicContainer>
{
    private readonly Dictionary<FieldDefinition, CilLocalVariable> _fieldsNonContextMapping = [];
    private ControlFlowGraph<Statement<CilInstruction>> _outerAst = null!;
    private CompilationUnit<CilInstruction> _compilationUnit = null!;

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

    public MoveNextToMethodLogicExtractor(
        StateMachine outerLogic,
        IInnerLogicContainerWithReturnFactory<StateMachine, MethodLogicContainer> innerLogicContainerFactory,
        TypeSignature returnType,
        GameModuleData gameModuleData,
        string methodName,
        CilInstruction innerFirstInstruction,
        CilInstruction innerLastInstruction)
        : base(
            outerLogic,
            innerLogicContainerFactory,
            [],
            returnType,
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
        // arg 0 for a state machine places its state machine type on the stack, but since we're extracting to a regular
        // method, that type never exists so we don't want that instruction in the inner logic. Since we don't want to
        // break labels that might be there, it's simpler to just replace the instruction with a Nop.
        if (instructionParameter.MethodSignatureIndex == 0)
            instruction.ReplaceWithNop();
        return EmptyIl;
    }

    protected override List<CilInstruction> ProcessLocalOperationInstruction(
        CilInstruction instruction,
        CilLocalVariable localVariable)
    {
        if (OuterLogic.Context is not null && OuterLogic.Context.ContextFieldsMapping.TryGetValue(
                localVariable,
                out FieldDefinition? fieldInContext))
        {
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

            if (instruction.IsStloc())
            {
                ProcessContextFieldStore(instruction);
                instruction.ReplaceWith(Stfld, fieldInContext);
                return EmptyIl;
            }
        }

        // The local 1 of a state machine is always the real "this" instance for instance state machine. This means we
        // need to map it to the inner's arg 0.
        if (OuterLogic.EnumeratorMethod.IsStatic || localVariable.Index != 1)
            return base.ProcessLocalOperationInstruction(instruction, localVariable);

        if (instruction.IsLdloc())
            instruction.ReplaceWith(Ldarg_0);
        else if (instruction.OpCode == Ldloca || instruction.OpCode == Ldloca_S)
            instruction.ReplaceWith(Ldarga, (byte)0);
        else if (instruction.IsStloc())
            instruction.ReplaceWith(Starg, (byte)0);

        return EmptyIl;
    }

    protected override void ProcessFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        if (OuterLogic.Context is not null && OuterLogic.Context.ContextFieldsMapping.TryGetValue(
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

        ProcessContextFieldStore(instruction);
        instruction.Operand = fieldInContext;
    }

    private void ProcessNonContextFieldOperationInstruction(
        CilInstruction instruction,
        FieldDefinition instructionField)
    {
        // Non context fields are mapped similarly to locals of a regular method.
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

    private void ProcessContextFieldStore(CilInstruction instruction)
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
                new(Ldarg_1),
                new(originalInstruction.OpCode, originalInstruction.Operand)
            ]);
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

    protected override IList<CilInstruction> GetReturnToOuterIl(
        bool afterLogicTransfer,
        List<CilInstruction> ilReturnValue) =>
    [
        .. ilReturnValue.Select(x => new CilInstruction(x.OpCode, x.Operand)),
        new(Ret)
    ];

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