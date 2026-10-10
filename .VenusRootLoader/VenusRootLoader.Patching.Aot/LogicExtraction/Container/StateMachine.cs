using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System.Collections;
using VenusRootLoader.Patching.Aot.LogicExtraction.Context;
using VenusRootLoader.Patching.Aot.LogicExtraction.Context.ContextSource;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicExtraction.Container;

/// <summary>
/// <p>
/// An abstraction of the construct the compiler generates when it encounters a method returning an <see cref="IEnumerator"/>
/// that contains at least one yield return statement. It contains properties to access all the interesting constructs
/// of a state machine mentioned below. It also contains utilities to easily create an abstraction from its enumerator method.
/// </p>
/// A state machine is composed of 2 components:
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
/// yield return, the code will also set the state to be the number whose associated code segment is right after the yield return.
/// This is how the compiler can create the machinery needed to have a suspend / resume flow on a method that must return
/// a value since it's seen as an iterator that's simply called multiple times.
/// </p>
/// <p>
/// All local variables used across multiple yield returns segment in the C# code gets promoted to be fields of the state
/// machine class. Additionally, if the method was an instance method, the "this" is stored as a field of the state machine
/// class that the enumerator method will set when called. The name of the class as well as all of its fields except for
/// the ones storing the arguments of the enumerator methods are unspeakable and must be resolved via reflection at runtime
/// to read or write from them. The local 0 of <see cref="IEnumerator.MoveNext"/> is always the state and if the enumerator
/// method is an instance one, its local 1 is always the "real this" instance (since arg 0 will always be the state machine type).
/// </p>
/// </summary>
public sealed class StateMachine : LogicContainer
{
    public const string StateFieldName = "<>1__state";
    public const string CurrentFieldName = "<>2__current";
    public const string ThisFieldName = "<>4__this";

    private GameModuleData GameModuleData { get; }
    private CilLocalVariable? _localCoroutine;

    public override MethodDefinition ReceivingMethod => MoveNextMethod;

    public override LogicContext? Context { get; protected set; }
    protected override TypeDefinition ContextDeclaringType => StateMachineType.DeclaringType!;
    protected override TypeSignature? ContextTypeSignature => ContextField?.Signature?.FieldType;

    /// <summary>
    /// The field of <see cref="StateMachineType"/> that contains the context information. If the context doesn't exist,
    /// this is null.
    /// </summary>
    public FieldDefinition? ContextField { get; private set; }

    /// <summary>
    /// The type of the state machine class which implements <see cref="IEnumerator"/>.
    /// </summary>
    public TypeDefinition StateMachineType { get; }

    /// <summary>
    /// The method that creates an instance of <see cref="StateMachineType"/> with the same signature and return type of
    /// the method that was originally declared in C#.
    /// </summary>
    public MethodDefinition EnumeratorMethod { get; }

    /// <summary>
    /// The <see cref="IEnumerator.MoveNext"/> method declared in the <see cref="StateMachineType"/>.
    /// </summary>
    public MethodDefinition MoveNextMethod { get; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the <see cref="IEnumerator.Current"/> value. This field
    /// has an unspeakable name.
    /// </summary>
    public FieldDefinition CurrentField { get; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the internal state value. This has field an
    /// unspeakable name.
    /// </summary>
    public FieldDefinition StateField { get; }

    /// <summary>
    /// The field of the <see cref="StateMachineType"/> that stores the "this" value if <see cref="EnumeratorMethod"/>.
    /// is an instance one. This has field an unspeakable name. A value of null means the MoveNext does not use its this,
    /// or the enumerator method is static.
    /// </summary>
    public FieldDefinition? ThisField { get; }

    public StateMachine(
        GameModuleData gameModuleData,
        TypeDefinition stateMachineType,
        MethodDefinition enumeratorMethod,
        MethodDefinition moveNextMethod,
        FieldDefinition currentField,
        FieldDefinition stateField,
        FieldDefinition? thisField)
    {
        GameModuleData = gameModuleData;
        StateMachineType = stateMachineType;
        EnumeratorMethod = enumeratorMethod;
        MoveNextMethod = moveNextMethod;
        CurrentField = currentField;
        StateField = stateField;
        ThisField = thisField;
    }

    /// <summary>
    /// Creates a <see cref="StateMachine"/> instance from the enumerator method component of the state machine. All the
    /// property's values will get inferred by collecting information about the method.
    /// </summary>
    /// <param name="gameModuleData">The game module data to use for this state machine.</param>
    /// <param name="enumeratorMethod">The method returning an <see cref="IEnumerator"/> that's part of the state machine.</param>
    /// <returns>A <see cref="StateMachine"/> containing all the information associated.</returns>
    public StateMachine(
        GameModuleData gameModuleData,
        MethodDefinition enumeratorMethod)
    {
        TypeDefinition stateMachineType = enumeratorMethod.DeclaringType!.NestedTypes
            .Single(type => type.Name is not null
                            && type.Name.Value.Contains($"<{enumeratorMethod.Name}>")
                            && enumeratorMethod.Parameters
                                .All(param => type.Fields
                                    .Select(field => field.Name!.ToString()).Contains(param.Name)));

        GameModuleData = gameModuleData;
        EnumeratorMethod = enumeratorMethod;
        StateMachineType = stateMachineType;
        MoveNextMethod = stateMachineType.Methods.Single(x => x.Name == nameof(IEnumerator.MoveNext));
        CurrentField = stateMachineType.Fields.Single(x => x.Name == CurrentFieldName);
        StateField = stateMachineType.Fields.Single(x => x.Name == StateFieldName);
        ThisField = stateMachineType.Fields.SingleOrDefault(x => x.Name == ThisFieldName);
    }

    /// <summary>
    /// Obtains the IL that will transfer control flow to another <see cref="StateMachine"/>.
    /// </summary>
    /// <param name="otherStateMachine">The <see cref="StateMachine"/> to yield return to. More specifically, it will use its
    /// <see cref="EnumeratorMethod"/> to yield return to.</param>
    /// <param name="fieldArguments">The arguments to pass to <see cref="EnumeratorMethod"/> of the
    /// <paramref name="otherStateMachine"/>.</param>
    /// <param name="stateNumber">The state number to set as part of the yield return. Its segment should be the one that
    /// gets executed on the next call to <see cref="IEnumerator.MoveNext"/>.</param>
    /// <param name="labelAfter">The label to branch past the instructions if the inner state machine yield breaks immediately.</param>
    /// <returns>The IL that performs a yield return to <paramref name="otherStateMachine"/>'s
    /// <see cref="EnumeratorMethod"/> passing <paramref name="fieldArguments"/> to it and setting the state to
    /// <paramref name="stateNumber"/>.</returns>
    /// <remarks>
    /// This will insert code that looks like the following:
    /// <code>
    /// Coroutine innerCoroutine = MonoBehaviour.StartCoroutine(enumeratorMethod(context));
    /// if (innerCoroutine != null)
    ///     yield return innerCoroutine;
    /// // Code continues...
    /// </code>
    /// The reason that a yield return enumeratorMethod isn't performed is because of a quirk with how Unity schedules
    /// yield returns. In the event that MoveNext returns false on the first invocation, Unity will still stall the coroutine
    /// until the next frame. This is problematic because it means that it is no longer semantically equivalent with a
    /// method call. Helpfully, if StartCoroutine is used instead, unity will return null if the first MoveNext returns
    /// false because it doesn't need to store any coroutine in its scheduler. We can then skip the yield return if we
    /// receive null which avoids this problem.
    /// </remarks>
    public List<CilInstruction> GetTransferToOtherStateMachineIl(
        StateMachine otherStateMachine,
        List<FieldDefinition> fieldArguments,
        int stateNumber,
        ICilLabel labelAfter)
    {
        // The code requires a local so we need to reserve one if it wasn't present already.
        if (_localCoroutine is null)
        {
            _localCoroutine = new(GameModuleData.CoroutineType.ToTypeSignature(false));
            MoveNextMethod.CilMethodBody!.LocalVariables.Add(_localCoroutine);
        }

        List<CilInstruction> il = [];
        if (!otherStateMachine.EnumeratorMethod.IsStatic)
        {
            il.Add(new(Ldloc_1));
            il.Add(new(Dup));
        }
        else
        {
            // StartCoroutine is an instance method so we need to select a surrogate for the call, we pick MainManager.instance
            // due to its globality in the game.
            il.Add(new(Ldsfld, GameModuleData.MainManagerInstance));
        }

        foreach (FieldDefinition fieldArgument in fieldArguments)
        {
            il.Add(new(Ldarg_0));
            il.Add(new(Ldfld, fieldArgument));
        }

        il.Add(new(Call, otherStateMachine.EnumeratorMethod));
        il.Add(new(Call, GameModuleData.StartCoroutineMethod));
        il.Add(new(Stloc, _localCoroutine));
        il.Add(new(Ldloc, _localCoroutine));
        il.Add(new(Brfalse, labelAfter));

        il.Add(new(Ldarg_0));
        il.Add(new(Ldloc, _localCoroutine));
        il.Add(new(Stfld, CurrentField));
        il.Add(new(Ldarg_0));
        il.Add(CilInstruction.CreateLdcI4(stateNumber));
        il.Add(new(Stfld, StateField));
        il.Add(CilInstruction.CreateLdcI4(1));
        il.Add(new(Ret));
        return il;
    }

    /// <summary>
    /// Obtains the IL needed for this state machine to call a method without yield return flow.
    /// </summary>
    /// <param name="otherMethod">The method to call.</param>
    /// <param name="fieldArguments">The fields to pass as arguments to the method.</param>
    /// <returns>The IL that calls a method without yield returning.</returns>
    public static List<CilInstruction> GetTransferToMethodIl(
        MethodLogicContainer otherMethod,
        List<FieldDefinition> fieldArguments)
    {
        List<CilInstruction> stateTransitionIl = [];
        if (!otherMethod.ReceivingMethod.IsStatic)
            stateTransitionIl.Add(new(Ldloc_1));

        foreach (FieldDefinition fieldArgument in fieldArguments)
        {
            stateTransitionIl.Add(new(Ldarg_0));
            stateTransitionIl.Add(new(Ldfld, fieldArgument));
        }

        stateTransitionIl.Add(new(Call, otherMethod.ReceivingMethod));
        return stateTransitionIl;
    }

    /// <summary>
    /// Creates a field to be added to the context using a field the <see cref="StateMachineType"/>.
    /// </summary>
    /// <param name="fieldSpeakableName">The speakable name of the field to use as source for the context field.</param>
    /// <param name="readOnly">Whether the context field will only be initialized without being commited back to the container.</param>
    /// <param name="mappedName">An optional name to use as the field of the context. This defaults to <paramref name="fieldSpeakableName"/></param>
    /// <returns>The new context field sourced from the field of <see cref="StateMachineType"/> whose speakable name is <paramref name="fieldSpeakableName"/>.</returns>
    public LogicContextField AddContextFieldFromSpeakableFieldName(
        string fieldSpeakableName,
        bool readOnly,
        string? mappedName = null)
    {
        return new LogicContextField
        {
            ContextFieldSources = [new FieldContextFieldSource(GetFieldFromSpeakableName(fieldSpeakableName))],
            FieldName = mappedName ?? fieldSpeakableName,
            ReadOnly = readOnly,
        };
    }

    private FieldDefinition GetFieldFromSpeakableName(string speakableName)
    {
        IList<FieldDefinition> fields = StateMachineType.Fields;
        // Since an argument is still converted to field with a speakable name, we have to try both matching schemes.
        FieldDefinition? exactMatch = fields.SingleOrDefault(x => x.Name == speakableName);
        FieldDefinition contextField = exactMatch ?? fields.Single(x =>
            x.Name is not null && x.Name.Value.StartsWith($"<{speakableName}>"));
        return contextField;
    }

    protected override void IntroduceContextInContainer(string name, TypeDefinition contextType)
    {
        ContextField = new(name, FieldAttributes.Private, contextType.ToTypeSignature());
        StateMachineType.Fields.Add(ContextField);
    }

    protected override List<CilInstruction> GetIlConstructContext(
        TypeDefinition contextType,
        List<CilInstruction> ilInitializeContext)
    {
        return
        [
            new(Ldarg_0),
            new(Newobj, contextType.GetConstructor()),
            .. ilInitializeContext,
            new(Stfld, ContextField)
        ];
    }

    protected override List<CilInstruction> GetIlPrepareContextForFieldCommit(IContextFieldSource fieldSource)
    {
        List<CilInstruction> il = [];
        // This is required for the stfld to work after this code.
        if (fieldSource is FieldContextFieldSource)
            il.Add(new(Ldarg_0));

        il.Add(new(Ldarg_0));
        il.Add(new(Ldfld, ContextField));
        return il;
    }
}