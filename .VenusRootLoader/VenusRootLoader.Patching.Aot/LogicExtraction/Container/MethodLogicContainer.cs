using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicExtraction.Context;
using VenusRootLoader.Patching.Aot.LogicExtraction.Context.ContextSource;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Container;

/// <summary>
/// A <see cref="LogicContainer"/> wrapping a regular method.
/// </summary>
public sealed class MethodLogicContainer : LogicContainer
{
    public override MethodDefinition ReceivingMethod { get; }
    public override LogicContext? Context { get; protected set; }
    protected override TypeDefinition ContextDeclaringType => ReceivingMethod.DeclaringType!;
    protected override TypeSignature? ContextTypeSignature => ContextLocal?.VariableType;

    /// <summary>
    /// The local generated in the method after patching the context. This is null if it hasn't been patched yet.
    /// </summary>
    public CilLocalVariable? ContextLocal { get; private set; }

    /// <summary>
    /// Creates a container that wraps an existing method.
    /// </summary>
    /// <param name="receivingMethod">The method this container will wrap.</param>
    public MethodLogicContainer(MethodDefinition receivingMethod) => ReceivingMethod = receivingMethod;

    /// <summary>
    /// Creates a field to be added to the context using an argument index of the container's method.
    /// </summary>
    /// <param name="argumentIndex">The argument index to use as source for the context field.</param>
    /// <param name="readOnly">Whether the context field will only be initialized without being commited back to the container.</param>
    /// <param name="mappedName">An optional name to use as the field of the context. This defaults to A_<paramref name="argumentIndex"/></param>
    /// <returns>The new context field sourced from the argument of the container at index <paramref name="argumentIndex"/>.</returns>
    public LogicContextField AddContextFieldFromArgumentIndex(
        int argumentIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new LogicContextField
        {
            ContextFieldSources = [new ArgumentContextFieldSource(ReceivingMethod.Parameters[argumentIndex])],
            FieldName = mappedName ?? "A_" + argumentIndex,
            ReadOnly = readOnly
        };
    }

    /// <summary>
    /// Creates a field to be added to the context using either an argument index or a local index of the container's method.
    /// </summary>
    /// <param name="argumentIndex">The argument index to use as source for the context field.</param>
    /// <param name="localIndex">The local index to use as source for the context field.</param>
    /// <param name="readOnly">Whether the context field will only be initialized without being commited back to the container.</param>
    /// <param name="mappedName">An optional name to use as the field of the context. This defaults to A_<paramref name="argumentIndex"/></param>
    /// <returns>The new context field sourced from either the argument of the container at index <paramref name="argumentIndex"/>.
    /// or the local variable of the container at index <paramref name="localIndex"/>.</returns>
    public LogicContextField AddContextFieldFromArgumentIndexAndLocalIndex(
        int argumentIndex,
        int localIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new LogicContextField
        {
            ContextFieldSources =
            [
                new ArgumentContextFieldSource(ReceivingMethod.Parameters[argumentIndex]),
                new LocalContextFieldSource(ReceivingMethod.CilMethodBody!.LocalVariables[localIndex])
            ],
            FieldName = mappedName ?? "A_" + argumentIndex,
            ReadOnly = readOnly
        };
    }

    /// <summary>
    /// Obtains the IL that calls a method with a similar signature as this container's method.
    /// </summary>
    /// <param name="otherMethod">The other method to call.</param>
    /// <param name="parameterArguments">The arguments to pass to the other method.</param>
    /// <returns>The IL calling the <paramref name="otherMethod"/>.</returns>
    public static List<CilInstruction> GetTransferToOtherMethodIl(
        MethodLogicContainer otherMethod,
        List<Parameter> parameterArguments)
    {
        List<CilInstruction> transferIl = [];
        if (!otherMethod.ReceivingMethod.IsStatic)
            transferIl.Add(new(Ldarg_0));

        foreach (Parameter parameterArgument in parameterArguments)
            transferIl.Add(new(Ldarg, parameterArgument));

        transferIl.Add(new(Call, otherMethod.ReceivingMethod));
        return transferIl;
    }

    /// <summary>
    /// Obtains the IL that calls a method passing local variable values as arguments.
    /// </summary>
    /// <param name="otherMethod">The other method to call.</param>
    /// <param name="localArguments">The local variables to pass as arguments to the other method.</param>
    /// <returns>The IL calling the <paramref name="otherMethod"/>.</returns>
    public static List<CilInstruction> GetTransferFromOuterMethodIl(
        MethodLogicContainer otherMethod,
        List<CilLocalVariable> localArguments)
    {
        List<CilInstruction> transferIl = [];
        if (!otherMethod.ReceivingMethod.IsStatic)
            transferIl.Add(new(Ldarg_0));

        foreach (CilLocalVariable localArgument in localArguments)
            transferIl.Add(new(Ldloc, localArgument));

        transferIl.Add(new(Call, otherMethod.ReceivingMethod));
        return transferIl;
    }

    protected override void IntroduceContextInContainer(string name, TypeDefinition contextType)
    {
        ContextLocal = new(contextType.ToTypeSignature());
        ReceivingMethod.CilMethodBody!.LocalVariables.Add(ContextLocal);
    }

    protected override List<CilInstruction> GetIlConstructContext(
        TypeDefinition contextType,
        List<CilInstruction> ilInitializeContext)
    {
        return
        [
            new(Newobj, contextType.GetConstructor()),
            .. ilInitializeContext,
            new(Stloc, ContextLocal)
        ];
    }

    protected override List<CilInstruction> GetIlPrepareContextForFieldCommit(IContextFieldSource fieldSource)
    {
        return [new(Ldloc, ContextLocal)];
    }
}