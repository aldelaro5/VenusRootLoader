using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

public sealed class MethodLogicContainer : ILogicContainer
{
    /// <summary>
    /// Represents a mapped context field of a <see cref="StateMachineContextInfo"/>.
    /// </summary>
    public sealed class MethodContextField
    {
        /// <summary>
        /// The local from the outer method to map to a context.
        /// </summary>
        public required CilLocalVariable ContextSource { get; init; }

        /// <summary>
        /// The name the context field will be mapped to. This can be different from the name of the <see cref="ContextSource"/>.
        /// </summary>
        public required string FieldName { get; init; }

        /// <summary>
        /// Whether the context field is only read from the inner method and never commited back to the
        /// outer method.
        /// </summary>
        public required bool ReadOnly { get; init; }
    }

    /// <summary>
    /// Contains all the information to initialize a context from an outer method, pass it to an inner
    /// method, and commit back to the outer method.
    /// </summary>
    public sealed class MethodContextInfo
    {
        /// <summary>
        /// The information about the fields to pass inside the context.
        /// </summary>
        public List<MethodContextField> ContextFields { get; } = new();

        /// <summary>
        /// The name of the type the context will have.
        /// </summary>
        public required string TypeName { get; init; }

        /// <summary>
        /// After patching the initialize and commit part of the context, this represents the mapping from the method
        /// locals to the context fields. This is empty if the context hasn't been patched yet.
        /// </summary>
        public Dictionary<CilLocalVariable, FieldDefinition> ContextFieldsMapping { get; } = new();

        /// <summary>
        /// The local generated in the method after patching the context. This is null if it hasn't been patched yet.
        /// </summary>
        public CilLocalVariable? ContextLocal { get; set; }
    }

    /// <summary>
    /// The current context if it exists. If it doesn't, this is null.
    /// </summary>
    public MethodContextInfo? ContextInfo { get; private set; }

    public MethodDefinition ReceivingMethod { get; }

    public NamedParameter? GetContextParameter()
    {
        if (ContextInfo?.ContextLocal is null)
            return null;

        return new()
        {
            Name = "context",
            TypeSignature = ContextInfo.ContextLocal.VariableType
        };
    }

    public MethodLogicContainer(MethodDefinition receivingMethod)
    {
        ReceivingMethod = receivingMethod;
    }

    public static List<CilInstruction> GetTransferToMethodIl(
        MethodLogicContainer otherMethod,
        List<Parameter> parameterArguments)
    {
        List<CilInstruction> stateTransitionIl = [];
        if (!otherMethod.ReceivingMethod.IsStatic)
            stateTransitionIl.Add(new(Ldarg_0));

        foreach (Parameter parameterArgument in parameterArguments)
            stateTransitionIl.Add(new(Ldarg, parameterArgument));

        stateTransitionIl.Add(new(Call, otherMethod.ReceivingMethod));
        return stateTransitionIl;
    }
}