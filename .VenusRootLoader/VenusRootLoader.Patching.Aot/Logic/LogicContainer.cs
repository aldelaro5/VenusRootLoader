using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using VenusRootLoader.Patching.Aot.ContextSource;
using VenusRootLoader.Patching.Aot.Logic.Context;
using VenusRootLoader.Patching.Aot.LogicExtraction;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.Logic;

/// <summary>
/// A construct that allows to contain logic in a method such that part of them can be extracted via an <see cref="LogicExtractor{TOuter,TInner}"/>.
/// Containers also have access to a <see cref="LogicContext"/> which allows an inner container to receive information from an outer container
/// and for the outer container to commit back the information changed by the inner container. The context is tracked by the
/// container implementation and its <see cref="LogicContextField"/> can be sourced from different schemes via an <see cref="IContextFieldSource"/>.
/// </summary>
public abstract class LogicContainer
{
    /// <summary>
    /// The method that contains the logic.
    /// </summary>
    public abstract MethodDefinition ReceivingMethod { get; }

    /// <summary>
    /// The current context if it exists. If it doesn't, this is null.
    /// </summary>
    public abstract LogicContext? Context { get; protected set; }

    /// <summary>
    /// The type the context will be placed under.
    /// </summary>
    protected abstract TypeDefinition ContextDeclaringType { get; }

    /// <summary>
    /// The type signature of the context if it exists. If it doesn't, this is null.
    /// </summary>
    protected abstract TypeSignature? ContextTypeSignature { get; }

    /// <summary>
    /// Discard the previous context if it exists and create a new one.
    /// </summary>
    /// <param name="typeName">The name the new context type will have.</param>
    /// <param name="contextFields">The fields information of the new context.</param>
    public void AssignNewContext(string typeName, List<LogicContextField> contextFields)
    {
        Context = new() { TypeName = typeName };
        Context.ContextFields.AddRange(contextFields);
    }

    /// <summary>
    /// Obtains a <see cref="!:NamedParameter"/> that represents the context of this container.
    /// </summary>
    /// <returns>A <see cref="!:NamedParameter"/> that represents the context of this container or null if this container
    /// has no context assigned to</returns>
    public NamedParameter? GetContextParameter()
    {
        if (ContextTypeSignature is null)
            return null;

        return new()
        {
            Name = "context",
            TypeSignature = ContextTypeSignature
        };
    }

    /// <summary>
    /// Link the context type with its container.
    /// </summary>
    /// <param name="name">A name to use as marker for the context if possible.</param>
    /// <param name="contextType">The type of the context to link to the container.</param>
    protected abstract void IntroduceContextInContainer(string name, TypeDefinition contextType);

    /// <summary>
    /// Obtains the IL needed to construct and initialize the context.
    /// </summary>
    /// <param name="contextType">The type of the context which has a parameterless constructor available.</param>
    /// <param name="ilInitializeContext">The IL that completes the initialization of the context after its constructor was called/</param>
    /// <returns>The IL that initializes the context fully.</returns>
    protected abstract List<CilInstruction> GetIlConstructContext(
        TypeDefinition contextType,
        List<CilInstruction> ilInitializeContext);

    /// <summary>
    /// Obtains the IL needed to prepare the context for commiting a field to its container.
    /// </summary>
    /// <param name="fieldSource">The source of the context field.</param>
    /// <returns>The IL that prepares the context for commiting a field.</returns>
    protected abstract List<CilInstruction> GetIlPrepareContextForFieldCommit(IContextFieldSource fieldSource);

    /// <summary>
    /// Creates a field to be added to the context using a local index of the container's method.
    /// </summary>
    /// <param name="localIndex">The local index to use as source for the context field.</param>
    /// <param name="readOnly">Whether the context field will only be initialized without being commited back to the container.</param>
    /// <param name="mappedName">An optional name to use as the field of the context. This defaults to V_<paramref name="localIndex"/></param>
    /// <returns>The new context field sourced from the local variable of the container at index <paramref name="localIndex"/>.</returns>
    public LogicContextField AddContextFieldFromLocalIndex(
        int localIndex,
        bool readOnly,
        string? mappedName = null)
    {
        return new LogicContextField
        {
            ContextFieldSources =
                [new LocalContextFieldSource(ReceivingMethod.CilMethodBody!.LocalVariables[localIndex])],
            FieldName = mappedName ?? "V_" + localIndex,
            ReadOnly = readOnly
        };
    }

    /// <summary>
    /// Patches this container to initialize and commit back a context that will be passed to a potential inner container.
    /// The type of the context will be created in the <see cref="ContextDeclaringType"/> type.
    /// </summary>
    /// <param name="gameModuleData">The module data used to create the context type.</param>
    /// <param name="initializeContextIlOffset">The IL offset to insert the initialization code of the context from the container.</param>
    /// <param name="commitContextIlOffset">The IL offset to insert the committing code of the context back to the container.</param>
    public void PatchContextIntoContainer(
        GameModuleData gameModuleData,
        int initializeContextIlOffset,
        int commitContextIlOffset)
    {
        if (Context is null)
            return;

        TypeDefinition contextType = CreateContextType(gameModuleData);

        Dictionary<IContextFieldSource, FieldDefinition> fieldsToCommitContextMapping = new();
        Dictionary<IContextFieldSource, FieldDefinition> fieldsToInitializeContextMapping = new();
        AddContextFields(contextType, fieldsToInitializeContextMapping, fieldsToCommitContextMapping);

        ContextDeclaringType.NestedTypes.Add(contextType);

        // Converts Pascal case to camelCase.
        string contextFieldName = char.ToLower(Context.TypeName[0]) + Context.TypeName[1..];
        IntroduceContextInContainer(contextFieldName, contextType);
        CilInstructionCollection methodIl = ReceivingMethod.CilMethodBody!.Instructions;
        PatchContextInitializeCode(initializeContextIlOffset, fieldsToInitializeContextMapping, methodIl, contextType);
        PatchContextCommitCode(commitContextIlOffset, fieldsToCommitContextMapping, methodIl);
    }

    private TypeDefinition CreateContextType(GameModuleData gameModuleData)
    {
        TypeDefinition contextType = new(
            ContextDeclaringType.Namespace,
            Context!.TypeName,
            TypeAttributes.NestedPublic
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AnsiClass,
            gameModuleData.Module.CorLibTypeFactory.Object.Type);
        MethodDefinition contextCtor = MethodDefinition.CreateConstructor(gameModuleData.Module, []);
        contextCtor.CilMethodBody!.Instructions.InsertRange(
            0,
            [
                new(Ldarg_0),
                new(Call, gameModuleData.ObjectConstructorMethod)
            ]);
        contextType.Methods.Add(contextCtor);
        return contextType;
    }

    private void AddContextFields(
        TypeDefinition contextType,
        Dictionary<IContextFieldSource, FieldDefinition> fieldsToInitializeContextMapping,
        Dictionary<IContextFieldSource, FieldDefinition> fieldsToCommitContextMapping)
    {
        foreach (LogicContextField logicContextField in Context!.ContextFields)
        {
            FieldDefinition newContextField = new(
                MakeUnspeakableNameSpeakable(logicContextField),
                FieldAttributes.Public,
                // All context field sources should always have the same underlying type.
                logicContextField.ContextFieldSources[0].TypeSignature);

            contextType.Fields.Add(newContextField);

            foreach (IContextFieldSource contextSource in logicContextField.ContextFieldSources)
                Context.ContextFieldsMapping.Add(contextSource.Key, newContextField);

            // We don't want to map each field multiple time so we just need the first one to use for the IL generation.
            fieldsToInitializeContextMapping.Add(logicContextField.ContextFieldSources[0], newContextField);
            if (!logicContextField.ReadOnly)
                fieldsToCommitContextMapping.Add(logicContextField.ContextFieldSources[0], newContextField);
        }
    }

    private static string MakeUnspeakableNameSpeakable(LogicContextField logicContextField)
    {
        string unspeakableName = logicContextField.FieldName;
        unspeakableName = unspeakableName.Replace("<", "");
        int indexOfClosingAngledBracket = unspeakableName.IndexOf('>');
        string speakableName = indexOfClosingAngledBracket > -1
            ? unspeakableName[..indexOfClosingAngledBracket]
            : unspeakableName;
        return speakableName;
    }

    private void PatchContextInitializeCode(
        int initializeContextIlOffset,
        Dictionary<IContextFieldSource, FieldDefinition> fieldsToInitializeContextMapping,
        CilInstructionCollection methodIl,
        TypeDefinition contextType)
    {
        List<CilInstruction> instructionsInitializeContext = new();
        foreach (KeyValuePair<IContextFieldSource, FieldDefinition> fieldMapping in fieldsToInitializeContextMapping)
        {
            // By this point, the instance of the context is on the stack so we can take advantage of its presence by
            // issuing a Dup so we can keep storing fields into it. The code will later pop the context once we're done
            // initializing it.
            instructionsInitializeContext.Add(new(Dup));
            instructionsInitializeContext.AddRange(fieldMapping.Key.GetLoadIl());
            instructionsInitializeContext.Add(new(Stfld, fieldMapping.Value));
        }

        methodIl.InsertRange(
            methodIl.GetIndexByOffset(initializeContextIlOffset),
            GetIlConstructContext(contextType, instructionsInitializeContext));
    }

    private void PatchContextCommitCode(
        int commitContextIlOffset,
        Dictionary<IContextFieldSource, FieldDefinition> fieldsToCommitContextMapping,
        CilInstructionCollection methodIl)
    {
        List<CilInstruction> instructionsWriteContext = new();
        foreach (KeyValuePair<IContextFieldSource, FieldDefinition> fieldMapping in fieldsToCommitContextMapping)
        {
            instructionsWriteContext.AddRange(GetIlPrepareContextForFieldCommit(fieldMapping.Key));
            instructionsWriteContext.Add(new(Ldfld, fieldMapping.Value));
            instructionsWriteContext.AddRange(fieldMapping.Key.GetStoreIl());
        }

        int commitContextIlIndex = methodIl.GetIndexByOffset(commitContextIlOffset);
        CilInstruction instructionCommitContext = methodIl[commitContextIlIndex];
        // This essentially prepends code before the instruction, but in such a way that the first instruction of our list
        // steals the original instruction's label so it preserves the original branching.
        methodIl.ReplaceRange(
            commitContextIlIndex,
            commitContextIlIndex,
            [
                .. instructionsWriteContext,
                new(instructionCommitContext.OpCode, instructionCommitContext.Operand)
            ]);
    }
}