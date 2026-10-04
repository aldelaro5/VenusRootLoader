using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System.Collections;

namespace VenusRootLoader.Patching.Aot.StateMachineUtils;

/// <summary>
/// Represents a mapped context field of a <see cref="StateMachineContextInfo"/>.
/// </summary>
internal sealed class StateMachineContextField
{
    /// <summary>
    /// The field from the outer <see cref="StateMachine"/> to map to a context.
    /// </summary>
    public required FieldDefinition Field { get; init; }

    /// <summary>
    /// The name the context field will be mapped to. This can be different from the name of the <see cref="Field"/>.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// Whether the context field is only read from the inner <see cref="StateMachine"/> and never commited back to the
    /// outer <see cref="StateMachine"/>.
    /// </summary>
    public required bool ReadOnly { get; init; }
}

/// <summary>
/// Contains all the information to initialize from an outer <see cref="StateMachine"/>, pass to an inner
/// <see cref="StateMachine"/>, and commit back to the outer <see cref="StateMachine"/>.
/// </summary>
internal sealed class StateMachineContextInfo
{
    public required string TypeName { get; init; }
    public required List<StateMachineContextField> Fields { get; init; }
}

/// <summary>
/// <p>
/// An abstraction of the construct the compiler generates when it encounters a method returning an <see cref="IEnumerator"/>
/// that contains at least one yield return statement. It contains properties to access all the interesting constructs
/// of a state machine mentioned below. It also contains utilities to easily create an abstraction from the method as well
/// as utilities to set up a context; a way for the state machine to receive information from an outer one and have the outer
/// state machine commit them back.
/// </p>
/// A state machine is composed of mainly 2 components:
/// <list type="bullet">
/// <item>A class with an unspeakable name implementing <see cref="IEnumerator"/> whose <see cref="IEnumerator.MoveNext"/>
/// contains the logic of the C# method with some compiler generated machinery for the yield return statements to function.</item>
/// <item>A method with the same signature and return type than the one declared in C#. Its logic only contains code to
/// initialize the state machine class mentioned above including the arguments the method receives since the compiler will
/// generate fields for each of them such that the method can commit the arguments values to them.</item>
/// </list>
/// <p>
/// Most of the machinery is done in the <see cref="IEnumerator.MoveNext"/> of the state machine class. Its code starts
/// with a switch called the state switch. The switch is done on a special field of the class called the state. Each arm
/// in that switch contains all the code segments that appears between all yield return statements. The arms all appear
/// in syntax order as they appeared in the C# code. Each segment ends with the code to perform a yield return (which
/// involves setting the <see cref="IEnumerator.Current"/> and returning true) or a yield break (returning false). On a
/// yield return, the code will also set the state to be the whose associated code segment is right after the yield return.
/// This is how the compiler can create the machinery needed to have a suspend / resume flow on a method that must return
/// a value since it's seen as an iterator that's simply called multiple times.
/// </p>
/// <p>
/// All local variables used across multiple yield returns segment in the C# code gets promoted to be fields of the state
/// machine class. Additionally, if the method was an instance method, the "this" is stored as a field of the state machine
/// class that the enumerator method will set when called. The name of the class as well as all of its fields except for
/// the ones storing the arguments of the enumerator methods are unspeakable and must be resolved via reflection at runtime
/// to read or write from them.
/// </p>
/// </summary>
internal sealed class StateMachine
{
    public const string StateFieldName = "<>1__state";
    public const string CurrentFieldName = "<>2__current";
    public const string ThisFieldName = "<>4__this";

    /// <summary>
    /// The type of the state machine class which implements <see cref="IEnumerator"/>.
    /// </summary>
    public required TypeDefinition StateMachineType { get; init; }

    /// <summary>
    /// The method that creates an instance of <see cref="StateMachineType"/> with the same signature and return type of
    /// the method that was originally declared in C#.
    /// </summary>
    public required MethodDefinition EnumeratorMethod { get; init; }

    /// <summary>
    /// The <see cref="IEnumerator.MoveNext"/> method declared in the <see cref="StateMachineType"/>.
    /// </summary>
    public required MethodDefinition MoveNextMethod { get; init; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the <see cref="IEnumerator.Current"/> value. This field
    /// has an unspeakable name.
    /// </summary>
    public required FieldDefinition CurrentField { get; init; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the internal state value. This has field an
    /// unspeakable name.
    /// </summary>
    public required FieldDefinition StateField { get; init; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the "this" value if <see cref="EnumeratorMethod"/>.
    /// is an instance one. This has field an unspeakable name. A value of null means the MoveNext does not use its this,
    /// or the enumerator method is static.
    /// </summary>
    public required FieldDefinition? ThisField { get; init; }

    /// <summary>
    /// Creates a <see cref="StateMachine"/> instance from the enumerator method component of the state machine. All the
    /// property's values will get inferred by collecting information about the method.
    /// </summary>
    /// <param name="enumeratorMethod">The method returning an <see cref="IEnumerator"/> that's part of the state machine.</param>
    /// <returns>A <see cref="StateMachine"/> containing all the information associated.</returns>
    public static StateMachine CreateFromEnumeratorMethod(MethodDefinition enumeratorMethod)
    {
        TypeDefinition stateMachineType = enumeratorMethod.DeclaringType!.NestedTypes
            .Single(type => type.Name is not null
                            && type.Name.Value.Contains($"<{enumeratorMethod.Name}>")
                            && enumeratorMethod.Parameters
                                .All(param => type.Fields
                                    .Select(field => field.Name!.ToString()).Contains(param.Name)));

        MethodDefinition moveNextMethod = stateMachineType.Methods.Single(x => x.Name == nameof(IEnumerator.MoveNext));

        FieldDefinition stateField = stateMachineType.Fields.Single(x => x.Name == StateFieldName);
        FieldDefinition currentField = stateMachineType.Fields.Single(x => x.Name == CurrentFieldName);
        FieldDefinition? thisField = stateMachineType.Fields.SingleOrDefault(x => x.Name == ThisFieldName);

        return new()
        {
            StateMachineType = stateMachineType,
            EnumeratorMethod = enumeratorMethod,
            MoveNextMethod = moveNextMethod,
            CurrentField = currentField,
            StateField = stateField,
            ThisField = thisField
        };
    }

    /// <summary>
    /// Creates a read / write <see cref="StateMachineContextField"/> that maps a field of this state machine to a context
    /// field for an inner <see cref="StateMachine"/> to receive and for that field to be commited back to the outer
    /// <see cref="StateMachine"/>.
    /// </summary>
    /// <param name="speakableName">A speakable version of the field name to map from this state machine.</param>
    /// <param name="mappedName">An optional name to use as the context field when mapping it. If it is null, the name will
    /// be a speakable version of the original name.</param>
    /// <returns>A <see cref="StateMachineContextField"/> with the matching field, name and with a
    /// <see cref="StateMachineContextField.ReadOnly"/> value of false.</returns>
    public StateMachineContextField GetContextFieldFromSpeakableName(string speakableName, string? mappedName = null)
    {
        return new()
        {
            Field = GetFieldFromSpeakableName(speakableName),
            FieldName = mappedName ?? speakableName,
            ReadOnly = false,
        };
    }

    /// <summary>
    /// Creates a read only <see cref="StateMachineContextField"/> that maps a field of this state machine to a context
    /// field for an inner <see cref="StateMachine"/> to receive.
    /// </summary>
    /// <param name="speakableName">A speakable version of the field name to map from this state machine.</param>
    /// <param name="mappedName">An optional name to use as the context field when mapping it. If it is null, the name will
    /// be a speakable version of the original name.</param>
    /// <returns>A <see cref="StateMachineContextField"/> with the matching field, name and with a
    /// <see cref="StateMachineContextField.ReadOnly"/> value of true.</returns>
    public StateMachineContextField GetReadOnlyContextFieldFromSpeakableName(
        string speakableName,
        string? mappedName = null)
    {
        return new()
        {
            Field = GetFieldFromSpeakableName(speakableName),
            FieldName = mappedName ?? speakableName,
            ReadOnly = true,
        };
    }

    private FieldDefinition GetFieldFromSpeakableName(string speakableName)
    {
        IList<FieldDefinition> fields = StateMachineType.Fields;
        FieldDefinition? exactMatch = fields.SingleOrDefault(x => x.Name == speakableName);
        FieldDefinition contextField = exactMatch ?? fields.Single(x =>
            x.Name is not null && x.Name.Value.StartsWith($"<{speakableName}>"));
        return contextField;
    }

    /// <summary>
    /// Obtains a list of <see cref="CilInstruction"/> that will perform a yield return to another <see cref="StateMachine"/>.
    /// </summary>
    /// <param name="otherStateMachine">The <see cref="StateMachine"/> to yield return to. More specifically, it will use its
    /// <see cref="EnumeratorMethod"/> to yield return to.</param>
    /// <param name="fieldArguments">The arguments to pass to <see cref="EnumeratorMethod"/> of the
    /// <paramref name="otherStateMachine"/>.</param>
    /// <param name="stateNumber">The state number to set as part of the yield return. Its segment should be the one that
    /// gets executed on the next call to <see cref="IEnumerator.MoveNext"/>.</param>
    /// <returns>A list of <see cref="CilInstruction"/> that performs a yield return to <paramref name="otherStateMachine"/>'s
    /// <see cref="EnumeratorMethod"/> passing <paramref name="fieldArguments"/> to it and setting the state to
    /// <paramref name="stateNumber"/>.</returns>
    public List<CilInstruction> GetYieldReturnToOtherStateMachineInstructions(
        StateMachine otherStateMachine,
        List<FieldDefinition> fieldArguments,
        int stateNumber)
    {
        List<CilInstruction> stateTransitionIl = [new(CilOpCodes.Ldarg_0)];
        if (!otherStateMachine.EnumeratorMethod.IsStatic)
            stateTransitionIl.Add(new(CilOpCodes.Ldloc_1));

        foreach (FieldDefinition fieldArgument in fieldArguments)
        {
            stateTransitionIl.Add(new(CilOpCodes.Ldarg_0));
            stateTransitionIl.Add(new(CilOpCodes.Ldfld, fieldArgument));
        }

        stateTransitionIl.Add(new(CilOpCodes.Call, otherStateMachine.EnumeratorMethod));
        stateTransitionIl.Add(new(CilOpCodes.Stfld, CurrentField));
        stateTransitionIl.Add(new(CilOpCodes.Ldarg_0));
        stateTransitionIl.Add(CilInstruction.CreateLdcI4(stateNumber));
        stateTransitionIl.Add(new(CilOpCodes.Stfld, StateField));
        stateTransitionIl.Add(CilInstruction.CreateLdcI4(1));
        stateTransitionIl.Add(new(CilOpCodes.Ret));
        return stateTransitionIl;
    }

    /// <summary>
    /// Patches this state machine's <see cref="MoveNextMethod"/> to initialise and commit back a context instance that
    /// will be passed to potential inner state machines. The type of the context will be created in the same declaring type
    /// as the <see cref="StateMachineType"/>. The context instance will be created in a new field of the state machine.
    /// </summary>
    /// <param name="referenceImporter">The reference importer to use.</param>
    /// <param name="stateMachineContextInfo">An object that contains all the information needed to generate a mapping
    /// from the state machine fields to their context fields counterpart.</param>
    /// <param name="initializeContextIlOffset">The IL offset to insert the initialization code of the context from the
    /// state machine.</param>
    /// <param name="commitContextIlOffset">The IL offset to insert the committing code of the context back to the state
    /// machine.</param>
    /// <param name="fieldsContextMapping">A dictionnary that will be amended with the final mappings of the fields from
    /// the state machine to their context counterpart.</param>
    /// <returns>The newly created field of the state machine that refers to the context instance.</returns>
    public FieldDefinition PatchStateMachineContextContext(
        ReferenceImporter referenceImporter,
        StateMachineContextInfo stateMachineContextInfo,
        int initializeContextIlOffset,
        int commitContextIlOffset,
        Dictionary<FieldDefinition, FieldDefinition> fieldsContextMapping)
    {
        TypeDefinition declaringType = StateMachineType.DeclaringType!;
        ModuleDefinition module = declaringType.DeclaringModule!;
        CilInstructionCollection moveNextIl = MoveNextMethod.CilMethodBody!.Instructions;

        TypeDefinition contextType = new(
            declaringType.Namespace,
            stateMachineContextInfo.TypeName,
            TypeAttributes.NestedPublic
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AnsiClass,
            module.CorLibTypeFactory.Object.Type);
        MethodDefinition contextCtor = MethodDefinition.CreateConstructor(module, []);
        contextCtor.CilMethodBody!.Instructions.InsertRange(
            0,
            [
                new(CilOpCodes.Ldarg_0),
                new(CilOpCodes.Call, referenceImporter.ImportMethod(typeof(object).GetConstructor([])!))
            ]);
        contextType.Methods.Add(contextCtor);

        Dictionary<FieldDefinition, FieldDefinition> fieldsToCommitContextMapping = new();
        foreach (StateMachineContextField stateMachineContextField in stateMachineContextInfo.Fields)
        {
            string unspeakableName = stateMachineContextField.FieldName;
            unspeakableName = unspeakableName.Replace("<", "");
            int indexOfClosingAngledBracket = unspeakableName.IndexOf('>');
            string speakableName = indexOfClosingAngledBracket > -1
                ? unspeakableName[..indexOfClosingAngledBracket]
                : unspeakableName;

            FieldDefinition newContextField = new(
                speakableName,
                FieldAttributes.Public,
                stateMachineContextField.Field.Signature);

            contextType.Fields.Add(newContextField);
            fieldsContextMapping.Add(stateMachineContextField.Field, newContextField);
            if (!stateMachineContextField.ReadOnly)
                fieldsToCommitContextMapping.Add(stateMachineContextField.Field, newContextField);
        }

        declaringType.NestedTypes.Add(contextType);

        // Converts Pascal case to camelCase.
        string contextFieldName =
            char.ToLower(stateMachineContextInfo.TypeName[0]) + stateMachineContextInfo.TypeName[1..];
        FieldDefinition contextField = new(
            contextFieldName,
            FieldAttributes.Private,
            contextType.ToTypeSignature());
        StateMachineType.Fields.Add(contextField);

        List<CilInstruction> instructionsInitializeContext = new();
        foreach (KeyValuePair<FieldDefinition, FieldDefinition> fieldMapping in fieldsContextMapping)
        {
            instructionsInitializeContext.Add(new(CilOpCodes.Dup));
            instructionsInitializeContext.Add(new(CilOpCodes.Ldarg_0));
            instructionsInitializeContext.Add(new(CilOpCodes.Ldfld, fieldMapping.Key));
            instructionsInitializeContext.Add(new(CilOpCodes.Stfld, fieldMapping.Value));
        }

        moveNextIl.InsertRange(
            moveNextIl.GetIndexByOffset(initializeContextIlOffset),
            [
                new(CilOpCodes.Ldarg_0),
                new(CilOpCodes.Newobj, contextCtor),
                .. instructionsInitializeContext,
                new(CilOpCodes.Stfld, contextField)
            ]);

        List<CilInstruction> instructionsWriteContext = new();
        foreach (KeyValuePair<FieldDefinition, FieldDefinition> fieldMapping in fieldsToCommitContextMapping)
        {
            instructionsWriteContext.Add(new(CilOpCodes.Ldarg_0));
            instructionsWriteContext.Add(new(CilOpCodes.Ldarg_0));
            instructionsWriteContext.Add(new(CilOpCodes.Ldfld, contextField));
            instructionsWriteContext.Add(new(CilOpCodes.Ldfld, fieldMapping.Value));
            instructionsWriteContext.Add(new(CilOpCodes.Stfld, fieldMapping.Key));
        }

        int indexSwitchEnd = moveNextIl.GetIndexByOffset(commitContextIlOffset);
        CilInstruction instructionSwitchEnd = moveNextIl[indexSwitchEnd];
        moveNextIl.ReplaceRange(
            indexSwitchEnd,
            indexSwitchEnd,
            [
                .. instructionsWriteContext,
                new(instructionSwitchEnd.OpCode, instructionSwitchEnd.Operand)
            ]);
        return contextField;
    }
}