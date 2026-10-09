using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Patching.Aot.LogicExtraction;
using FieldAttributes = AsmResolver.PE.DotNet.Metadata.Tables.FieldAttributes;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.MethodAttributes;
using PropertyAttributes = AsmResolver.PE.DotNet.Metadata.Tables.PropertyAttributes;
using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.TypeAttributes;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

/// <summary>
/// This class allows to create a <see cref="StateMachine"/> containing the basic structure where the state machine is
/// intended to serve as an inner portion of an outer one. The <see cref="IEnumerator.MoveNext"/> will be left with a
/// dummy setup of its state switch. It is meant to be filled by an <see cref="LogicExtractor{TOuter,TInner}"/>.
/// </summary>
public sealed class StateMachineInnerLogicContainerFactory : IInnerLogicContainerFactory<StateMachine, StateMachine>
{
    public StateMachine Create(
        StateMachine outerContainer,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string name)
    {
        StateMachine stateMachine = CreateAndAddInnerStateMachineType(
            outerContainer,
            gameModuleData,
            parameters,
            name);
        CilLocalVariable localState = new(stateMachine.StateField.Signature!.FieldType);
        CilLocalVariable? localThis = stateMachine.ThisField is not null
            ? new(stateMachine.ThisField.Signature!.FieldType)
            : null;
        CreateMoveNextBodyWithStateSwitchSetupInstructions(
            stateMachine,
            localState,
            localThis);
        return stateMachine;
    }

    /// <summary>
    /// Creates a new <see cref="StateMachine"/> that will serve to implement a portion of a bigger one. Its components
    /// will be added to the same declaring type as the outer state machine.
    /// </summary>
    /// <param name="outerStateMachine">The outer state machine of the one to create.</param>
    /// <param name="gameModuleData">The <see cref="parameters"/> to use for creating the state machine.</param>
    /// <param name="parameters">The parameters the enumerator method will have.</param>
    /// <param name="speakableName">The speakable version of the name of the state machine which will be the name of the
    /// enumerator method.</param>
    /// <returns>A new state machine with an empty <see cref="IEnumerator"/> that is set up to implement a smaller
    /// portion of the <paramref name="outerStateMachine"/>.</returns>
    private static StateMachine CreateAndAddInnerStateMachineType(
        StateMachine outerStateMachine,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string speakableName)
    {
        TypeDefinition declaringType = outerStateMachine.StateMachineType.DeclaringType!;
        IMethodDefOrRef debuggerHiddenAttributeCtor = (IMethodDefOrRef)gameModuleData.DebuggerHiddenAttributeCtor;

        TypeDefinition stateMachineType = new(
            declaringType.Namespace,
            $"<{speakableName}>",
            TypeAttributes.NestedPrivate
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AnsiClass,
            gameModuleData.Module.CorLibTypeFactory.Object.Type);

        AddCustomAttributesToStateMachine(gameModuleData, stateMachineType);

        FieldDefinition stateField = new(
            StateMachine.StateFieldName,
            FieldAttributes.Private,
            gameModuleData.Module.CorLibTypeFactory.Int32);
        stateMachineType.Fields.Add(stateField);
        FieldDefinition currentField = new(
            StateMachine.CurrentFieldName,
            FieldAttributes.Private,
            gameModuleData.Module.CorLibTypeFactory.Object);
        stateMachineType.Fields.Add(currentField);

        FieldDefinition? thisField = null;
        if (outerStateMachine.ThisField is not null)
        {
            thisField = new(StateMachine.ThisFieldName, FieldAttributes.Public, declaringType.ToTypeSignature());
            stateMachineType.Fields.Add(thisField);
        }

        foreach (NamedParameter namedParameter in parameters)
        {
            FieldDefinition paramField = new(namedParameter.Name, FieldAttributes.Public, namedParameter.TypeSignature);
            stateMachineType.Fields.Add(paramField);
        }

        AddMethodsAndPropertiesToStateMachine(
            gameModuleData,
            stateMachineType,
            debuggerHiddenAttributeCtor,
            stateField,
            currentField,
            out MethodDefinition moveNextMethod);

        MethodDefinition enumeratorMethod = CreateStateMachineMethod(
            gameModuleData,
            speakableName,
            parameters,
            stateMachineType,
            thisField,
            outerStateMachine.EnumeratorMethod.IsStatic);

        StateMachine stateMachine = new(
            gameModuleData,
            stateMachineType,
            enumeratorMethod,
            moveNextMethod,
            currentField,
            stateField,
            thisField);

        declaringType.NestedTypes.Add(stateMachine.StateMachineType);
        declaringType.Methods.Add(stateMachine.EnumeratorMethod);

        return stateMachine;
    }

    private static MethodDefinition CreateStateMachineMethod(
        GameModuleData gameModuleData,
        string methodName,
        List<NamedParameter> parameters,
        TypeDefinition stateMachineType,
        FieldDefinition? stateMachineThisField,
        bool isStatic)
    {
        MethodDefinition stateMachineConstructor =
            stateMachineType.GetConstructor([gameModuleData.Module.CorLibTypeFactory.Int32])!;

        Func<TypeSignature, IEnumerable<TypeSignature>, MethodSignature> signatureCreator = isStatic
            ? MethodSignature.CreateStatic
            : MethodSignature.CreateInstance;
        MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
        if (isStatic)
            methodAttributes |= MethodAttributes.Static;
        MethodDefinition methodDefinition = new(
            methodName,
            methodAttributes,
            signatureCreator(
                gameModuleData.EnumeratorType.ToTypeSignature(false),
                parameters.Select(x => x.TypeSignature)));

        for (int i = 0; i < parameters.Count; i++)
        {
            Parameter contextParameter = methodDefinition.Parameters[i];
            contextParameter.GetOrCreateDefinition().Name = parameters[i].Name;
        }

        IMethodDefOrRef methodDescriptor = (IMethodDefOrRef)gameModuleData.IteratorStateMachineAttributeCtor;
        CustomAttributeArgument stateMachineTypeArgument = new(
            gameModuleData.TypeType.ToTypeSignature(false),
            stateMachineType.ToTypeSignature());
        CustomAttribute iteratorStateMachineAttribute = new(
            methodDescriptor,
            new(stateMachineTypeArgument));
        methodDefinition.CustomAttributes.Add(iteratorStateMachineAttribute);

        methodDefinition.CilMethodBody = new()
        {
            Instructions =
            {
                Ldc_I4_0,
                { Newobj, stateMachineConstructor }
            }
        };

        CilInstructionCollection il = methodDefinition.CilMethodBody.Instructions;
        if (stateMachineThisField is not null)
        {
            il.Add(Dup);
            il.Add(Ldarg_0);
            il.Add(Stfld, stateMachineThisField);
        }

        foreach (Parameter parameter in methodDefinition.Parameters)
        {
            il.Add(Dup);
            il.Add(Ldarg, parameter);
            il.Add(Stfld, stateMachineType.Fields.Single(x => x.Name == parameter.Name));
        }

        il.Add(Ret);
        il.OptimizeMacros();

        return methodDefinition;
    }

    private static void AddCustomAttributesToStateMachine(
        GameModuleData gameModuleData,
        TypeDefinition stateMachineType)
    {
        ICustomAttributeType compilerGeneratedCtor =
            (ICustomAttributeType)gameModuleData.CompilerGeneratedAttributeCotr;
        stateMachineType.CustomAttributes.Add(new(compilerGeneratedCtor));
    }

    private static void AddMethodsAndPropertiesToStateMachine(
        GameModuleData gameModuleData,
        TypeDefinition stateMachine,
        IMethodDefOrRef debuggerHiddenAttributeCtor,
        FieldDefinition stateField,
        FieldDefinition currentField,
        out MethodDefinition moveNextMethod)
    {
        Type objectType = typeof(object);
        AddConstructorToStateMachine(
            gameModuleData,
            stateField,
            debuggerHiddenAttributeCtor,
            stateMachine);

        Type iEnumeratorObjectType = typeof(IEnumerator<object>);
        Type iEnumeratorType = typeof(IEnumerator);
        Type iDisposableType = typeof(IDisposable);

        stateMachine.Interfaces.Add(new(gameModuleData.EnumeratorObjectType));
        stateMachine.Interfaces.Add(new(gameModuleData.EnumeratorType));
        stateMachine.Interfaces.Add(new(gameModuleData.DisposableType));

        MethodInfo baseDisposeMethod = iDisposableType.GetMethod(nameof(IDisposable.Dispose))!;
        AddMethodImplementationToStateMachine(
            $"{iDisposableType.FullName}.{baseDisposeMethod.Name}",
            MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Void),
            false,
            (IMethodDefOrRef)gameModuleData.DisposableDispose,
            [new(debuggerHiddenAttributeCtor)],
            new() { Instructions = { Ret } },
            stateMachine);

        moveNextMethod = AddMethodImplementationToStateMachine(
            nameof(IEnumerator.MoveNext),
            MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Boolean),
            false,
            (IMethodDefOrRef)gameModuleData.EnumeratorMoveNext,
            [],
            null,
            stateMachine);

        MethodDefinition getCurrentObject = AddMethodImplementationToStateMachine(
            $"{iEnumeratorObjectType.Namespace}.{nameof(IEnumerator<>)}<{objectType.FullName}>.get_{nameof(IEnumerator<>.Current)}",
            MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Object),
            true,
            (IMethodDefOrRef)gameModuleData.EnumeratorObjectGetCurrent,
            [new(debuggerHiddenAttributeCtor)],
            new()
            {
                Instructions =
                {
                    Ldarg_0,
                    { Ldfld, currentField },
                    Ret
                }
            },
            stateMachine);

        AddMethodImplementationToStateMachine(
            $"{iEnumeratorType.FullName}.{nameof(IEnumerator.Reset)}",
            MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Void),
            false,
            (IMethodDefOrRef)gameModuleData.EnumeratorReset,
            [new(debuggerHiddenAttributeCtor)],
            new()
            {
                Instructions =
                {
                    { Newobj, gameModuleData.NotSupportedExceptionCtor },
                    Throw
                }
            },
            stateMachine);

        MethodDefinition getCurrent = AddMethodImplementationToStateMachine(
            $"{iEnumeratorType.FullName}.get_{nameof(IEnumerator<>.Current)}",
            MethodSignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Object),
            true,
            (IMethodDefOrRef)gameModuleData.EnumeratorGetCurrent,
            [new(debuggerHiddenAttributeCtor)],
            new()
            {
                Instructions =
                {
                    Ldarg_0,
                    { Ldfld, currentField },
                    Ret
                }
            },
            stateMachine);

        AddPropertyToStateMachine(
            $"{iEnumeratorObjectType.Namespace}.{nameof(IEnumerator<>)}<{objectType.FullName}>.{nameof(IEnumerator.Current)}",
            PropertySignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Object),
            getCurrentObject,
            null,
            stateMachine);

        AddPropertyToStateMachine(
            $"{iEnumeratorType.Namespace}.{nameof(IEnumerator)}.{nameof(IEnumerator.Current)}",
            PropertySignature.CreateInstance(gameModuleData.Module.CorLibTypeFactory.Object),
            getCurrent,
            null,
            stateMachine);
    }

    private static void AddPropertyToStateMachine(
        string propertyName,
        PropertySignature propertySignature,
        MethodDefinition getMethod,
        MethodDefinition? setMethod,
        TypeDefinition stateMachine)
    {
        PropertyDefinition getCurrentProp = new(
            propertyName,
            PropertyAttributes.None,
            propertySignature);
        getCurrentProp.GetMethod = getMethod;

        if (setMethod is not null)
            getCurrentProp.SetMethod = setMethod;

        stateMachine.Properties.Add(getCurrentProp);
    }

    private static void AddConstructorToStateMachine(
        GameModuleData gameModuleData,
        FieldDefinition stateField,
        IMethodDefOrRef debuggerHiddenAttributeCtor,
        TypeDefinition stateMachineType)
    {
        MethodDefinition ctor = MethodDefinition.CreateConstructor(
            gameModuleData.Module.CorLibTypeFactory,
            [gameModuleData.Module.CorLibTypeFactory.Int32]);
        ctor.Attributes |= MethodAttributes.HideBySig;
        ctor.CustomAttributes.Add(new(debuggerHiddenAttributeCtor));
        ctor.Parameters[0].GetOrCreateDefinition().Name = stateField.Name;
        ctor.CilMethodBody = new()
        {
            Instructions =
            {
                Ldarg_0,
                { Call, gameModuleData.ObjectConstructorMethod },
                Ldarg_0,
                Ldarg_1,
                { Stfld, stateField },
                Ret
            }
        };
        stateMachineType.Methods.Add(ctor);
    }

    private static MethodDefinition AddMethodImplementationToStateMachine(
        string methodName,
        MethodSignature methodSignature,
        bool isSpecialName,
        IMethodDefOrRef baseMethodRef,
        IEnumerable<CustomAttribute> attributeConstructors,
        CilMethodBody? body,
        TypeDefinition stateMachine)
    {
        MethodDefinition methodDefinition = new(
            methodName,
            MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.Virtual,
            methodSignature);
        if (isSpecialName)
            methodDefinition.Attributes |= MethodAttributes.SpecialName;

        foreach (CustomAttribute methodDefOrRef in attributeConstructors)
            methodDefinition.CustomAttributes.Add(methodDefOrRef);
        methodDefinition.CilMethodBody = body;

        stateMachine.Methods.Add(methodDefinition);
        // Required so that the IL knows that the method is an override.
        stateMachine.MethodImplementations.Add(new(baseMethodRef, methodDefinition));

        return methodDefinition;
    }

    private static void CreateMoveNextBodyWithStateSwitchSetupInstructions(
        StateMachine innerStateMachine,
        CilLocalVariable localState,
        CilLocalVariable? localThis)
    {
        CilInstruction stateSwitchInstruction = new(Switch);
        innerStateMachine.MoveNextMethod.CilMethodBody = new()
        {
            InitializeLocals = true,
            LocalVariables = { localState },
            Instructions =
            {
                Ldarg_0,
                { Ldfld, innerStateMachine.StateField },
                { Stloc, localState }
            }
        };

        if (localThis is not null)
            innerStateMachine.MoveNextMethod.CilMethodBody.LocalVariables.Add(localThis);

        if (localThis is not null)
        {
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldarg_0);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldfld, innerStateMachine.ThisField!);
            innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Stloc, localThis);
        }

        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldloc, localState);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(stateSwitchInstruction);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ldc_I4_0);
        innerStateMachine.MoveNextMethod.CilMethodBody.Instructions.Add(Ret);
    }
}