using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using VenusRootLoader.Patching.Aot.ContextSource;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

public sealed class MethodLogicContainer : ILogicContainer
{
    /// <summary>
    /// Represents a mapped context field of a <see cref="StateMachine.StateMachineContextInfo"/>.
    /// </summary>
    public sealed class MethodContextField
    {
        /// <summary>
        /// The local from the outer method to map to a context.
        /// </summary>
        public required List<IContextSource> ContextSources { get; init; }

        /// <summary>
        /// The name the context field will be mapped to. This can be different from the name of the <see cref="ContextSources"/>.
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
        public Dictionary<object, FieldDefinition> ContextFieldsMapping { get; } = new();

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

    public MethodContextField AddContextFieldFromLocalIndex(
        int localIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new MethodContextField
        {
            ContextSources = [new LocalContextSource(ReceivingMethod.CilMethodBody!.LocalVariables[localIndex])],
            FieldName = mappedName ?? "V_" + localIndex,
            ReadOnly = readOnly
        };
    }

    public MethodContextField AddContextFieldFromArgumentIndex(
        int argumentIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new MethodContextField
        {
            ContextSources = [new ArgumentContextSource(ReceivingMethod.Parameters[argumentIndex])],
            FieldName = mappedName ?? "A_" + argumentIndex,
            ReadOnly = readOnly
        };
    }

    public MethodContextField AddContextFieldFromArgumentIndexAndLocalIndex(
        int argumentIndex,
        int localIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new MethodContextField
        {
            ContextSources =
            [
                new ArgumentContextSource(ReceivingMethod.Parameters[argumentIndex]),
                new LocalContextSource(ReceivingMethod.CilMethodBody!.LocalVariables[localIndex])
            ],
            FieldName = mappedName ?? "A_" + argumentIndex,
            ReadOnly = readOnly
        };
    }

    public void AssignNewContext(string typeName, List<MethodContextField> contextFields)
    {
        ContextInfo = new() { TypeName = typeName };
        ContextInfo.ContextFields.AddRange(contextFields);
    }

    public void PatchMethodContext(
        GameModuleData gameModuleData,
        int initializeContextIlOffset,
        int commitContextIlOffset)
    {
        if (ContextInfo is null)
            return;

        TypeDefinition declaringType = ReceivingMethod.DeclaringType!;
        ModuleDefinition module = declaringType.DeclaringModule!;
        CilInstructionCollection methodIl = ReceivingMethod.CilMethodBody!.Instructions;

        TypeDefinition contextType = new(
            declaringType.Namespace,
            ContextInfo.TypeName,
            TypeAttributes.NestedPublic
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AnsiClass,
            module.CorLibTypeFactory.Object.Type);
        MethodDefinition contextCtor = MethodDefinition.CreateConstructor(module, []);
        contextCtor.CilMethodBody!.Instructions.InsertRange(
            0,
            [
                new(Ldarg_0),
                new(Call, gameModuleData.ObjectConstructorMethod)
            ]);
        contextType.Methods.Add(contextCtor);

        Dictionary<object, FieldDefinition> fieldsToCommitContextMapping = new();
        Dictionary<object, FieldDefinition> fieldsToInitializeContextMapping = new();
        foreach (MethodContextField methodContextField in ContextInfo.ContextFields)
        {
            FieldDefinition newContextField = new(
                methodContextField.FieldName,
                FieldAttributes.Public,
                methodContextField.ContextSources[0].TypeSignature);

            contextType.Fields.Add(newContextField);

            foreach (IContextSource contextSource in methodContextField.ContextSources)
                ContextInfo.ContextFieldsMapping.Add(contextSource.MappingKey, newContextField);

            fieldsToInitializeContextMapping.Add(methodContextField.ContextSources[0].Key, newContextField);
            if (!methodContextField.ReadOnly)
                fieldsToCommitContextMapping.Add(methodContextField.ContextSources[0].Key, newContextField);
        }

        declaringType.NestedTypes.Add(contextType);

        ContextInfo.ContextLocal = new(contextType.ToTypeSignature());
        ReceivingMethod.CilMethodBody!.LocalVariables.Add(ContextInfo.ContextLocal);

        List<CilInstruction> instructionsInitializeContext = new();
        foreach (KeyValuePair<object, FieldDefinition> fieldMapping in fieldsToInitializeContextMapping)
        {
            instructionsInitializeContext.Add(new(Dup));

            switch (fieldMapping.Key)
            {
                case Parameter:
                    instructionsInitializeContext.Add(new(Ldarg, fieldMapping.Key));
                    break;
                case CilLocalVariable:
                    instructionsInitializeContext.Add(new(Ldloc, fieldMapping.Key));
                    break;
            }

            instructionsInitializeContext.Add(new(Stfld, fieldMapping.Value));
        }

        methodIl.InsertRange(
            methodIl.GetIndexByOffset(initializeContextIlOffset),
            [
                new(Newobj, contextCtor),
                .. instructionsInitializeContext,
                new(Stloc, ContextInfo.ContextLocal)
            ]);

        List<CilInstruction> instructionsWriteContext = new();
        foreach (KeyValuePair<object, FieldDefinition> fieldMapping in fieldsToCommitContextMapping)
        {
            instructionsWriteContext.Add(new(Ldloc, ContextInfo.ContextLocal));
            instructionsWriteContext.Add(new(Ldfld, fieldMapping.Value));

            switch (fieldMapping.Key)
            {
                case Parameter:
                    instructionsWriteContext.Add(new(Starg, fieldMapping.Key));
                    break;
                case CilLocalVariable:
                    instructionsWriteContext.Add(new(Stloc, fieldMapping.Key));
                    break;
            }
        }

        int indexSwitchEnd = methodIl.GetIndexByOffset(commitContextIlOffset);
        CilInstruction instructionSwitchEnd = methodIl[indexSwitchEnd];
        methodIl.ReplaceRange(
            indexSwitchEnd,
            indexSwitchEnd,
            [
                .. instructionsWriteContext,
                new(instructionSwitchEnd.OpCode, instructionSwitchEnd.Operand)
            ]);
    }

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
}