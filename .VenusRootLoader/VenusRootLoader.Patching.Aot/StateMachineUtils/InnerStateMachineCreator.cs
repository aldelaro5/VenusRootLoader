using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using FieldAttributes = AsmResolver.PE.DotNet.Metadata.Tables.FieldAttributes;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.MethodAttributes;
using PropertyAttributes = AsmResolver.PE.DotNet.Metadata.Tables.PropertyAttributes;
using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.TypeAttributes;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.StateMachineUtils;

/// <summary>
/// This class allows to create a <see cref="StateMachine"/> containing the basic structure where the state machine is
/// intended to serve as an inner portion of an outer one. The <see cref="IEnumerator.MoveNext"/> will be left empty as
/// it is meant to be filled by the consumer that requested the state machine creation, typically by using a
/// <see cref="MoveNextLogicExtractor"/>.
/// </summary>
internal sealed class InnerStateMachineCreator
{
    /// <summary>
    /// Creates a new <see cref="StateMachine"/> that will serve to implement a portion of a bigger one. Its components
    /// will be added to the same declaring type as the outer state machine.
    /// </summary>
    /// <param name="outerStateMachine">The outer state machine of the one to create.</param>
    /// <param name="referenceImporter">The <see cref="ReferenceImporter"/> to use for creating the state machine.</param>
    /// <param name="parameters">The parameters the enumerator method will have.</param>
    /// <param name="speakableName">The speakable version of the name of the state machine which will be the name of the
    /// enumerator method.</param>
    /// <returns>A new state machine with an empty <see cref="IEnumerator.MoveNext"/> that is set up to implement a smaller
    /// portion of the <paramref name="outerStateMachine"/>.</returns>
    public static StateMachine CreateAndAddInnerStateMachineType(
        StateMachine outerStateMachine,
        ReferenceImporter referenceImporter,
        List<NamedParameter> parameters,
        string speakableName)
    {
        ModuleDefinition module = outerStateMachine.StateMachineType.DeclaringModule!;
        TypeDefinition declaringType = outerStateMachine.StateMachineType.DeclaringType!;
        IMethodDefOrRef debuggerHiddenAttributeCtor = (IMethodDefOrRef)referenceImporter
            .ImportMethod(typeof(DebuggerHiddenAttribute).GetConstructor([])!);

        TypeDefinition stateMachineType = new(
            declaringType.Namespace,
            $"<{speakableName}>",
            TypeAttributes.NestedPrivate
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit
            | TypeAttributes.AnsiClass,
            module.CorLibTypeFactory.Object.Type);

        AddCustomAttributesToStateMachine(referenceImporter, stateMachineType);

        FieldDefinition stateField = new(
            StateMachine.StateFieldName,
            FieldAttributes.Private,
            module.CorLibTypeFactory.Int32);
        stateMachineType.Fields.Add(stateField);
        FieldDefinition currentField = new(
            StateMachine.CurrentFieldName,
            FieldAttributes.Private,
            module.CorLibTypeFactory.Object);
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
            module,
            referenceImporter,
            stateMachineType,
            debuggerHiddenAttributeCtor,
            stateField,
            currentField,
            out MethodDefinition moveNextMethod);

        MethodDefinition enumeratorMethod = CreateStateMachineMethod(
            module,
            referenceImporter,
            speakableName,
            parameters,
            stateMachineType,
            thisField,
            outerStateMachine.EnumeratorMethod.IsStatic);

        StateMachine stateMachine = new()
        {
            StateMachineType = stateMachineType,
            EnumeratorMethod = enumeratorMethod,
            MoveNextMethod = moveNextMethod,
            CurrentField = currentField,
            StateField = stateField,
            ThisField = thisField,
        };

        declaringType.NestedTypes.Add(stateMachine.StateMachineType);
        declaringType.Methods.Add(stateMachine.EnumeratorMethod);

        return stateMachine;
    }

    private static MethodDefinition CreateStateMachineMethod(
        ModuleDefinition module,
        ReferenceImporter referenceImporter,
        string methodName,
        List<NamedParameter> parameters,
        TypeDefinition stateMachineType,
        FieldDefinition? stateMachineThisField,
        bool isStatic)
    {
        MethodDefinition stateMachineConstructor = stateMachineType.GetConstructor([module.CorLibTypeFactory.Int32])!;

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
                referenceImporter.ImportTypeSignature(typeof(IEnumerator)),
                parameters.Select(x => x.TypeSignature)));

        for (int i = 0; i < parameters.Count; i++)
        {
            Parameter contextParameter = methodDefinition.Parameters[i];
            contextParameter.GetOrCreateDefinition().Name = parameters[i].Name;
        }

        Type typeType = typeof(Type);
        IMethodDefOrRef methodDescriptor = (IMethodDefOrRef)referenceImporter
            .ImportMethod(typeof(IteratorStateMachineAttribute).GetConstructor([typeType])!);
        CustomAttributeArgument stateMachineTypeArgument = new(
            referenceImporter.ImportTypeSignature(typeType),
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
        ReferenceImporter referenceImporter,
        TypeDefinition stateMachineType)
    {
        ICustomAttributeType compilerGeneratedCtor = (ICustomAttributeType)referenceImporter
            .ImportMethod(typeof(CompilerGeneratedAttribute).GetConstructor([])!);
        stateMachineType.CustomAttributes.Add(new(compilerGeneratedCtor));
    }

    private static void AddMethodsAndPropertiesToStateMachine(
        ModuleDefinition module,
        ReferenceImporter referenceImporter,
        TypeDefinition stateMachine,
        IMethodDefOrRef debuggerHiddenAttributeCtor,
        FieldDefinition stateField,
        FieldDefinition currentField,
        out MethodDefinition moveNextMethod)
    {
        Type objectType = typeof(object);
        AddConstructorToStateMachine(
            module,
            referenceImporter,
            stateField,
            debuggerHiddenAttributeCtor,
            objectType.GetConstructor([])!,
            stateMachine);

        Type iEnumeratorObjectType = typeof(IEnumerator<object>);
        Type iEnumeratorType = typeof(IEnumerator);
        Type iDisposableType = typeof(IDisposable);

        stateMachine.Interfaces.Add(new(referenceImporter.ImportType(iEnumeratorObjectType)));
        stateMachine.Interfaces.Add(new(referenceImporter.ImportType(iEnumeratorType)));
        stateMachine.Interfaces.Add(new(referenceImporter.ImportType(iDisposableType)));

        MethodInfo baseDisposeMethod = iDisposableType.GetMethod(nameof(IDisposable.Dispose))!;
        AddMethodImplementationToStateMachine(
            referenceImporter,
            $"{iDisposableType.FullName}.{baseDisposeMethod.Name}",
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Void),
            false,
            baseDisposeMethod,
            [new(debuggerHiddenAttributeCtor)],
            new() { Instructions = { Ret } },
            stateMachine);

        MethodInfo baseMoveNextMethod = iEnumeratorType.GetMethod(nameof(IEnumerator.MoveNext))!;
        moveNextMethod = AddMethodImplementationToStateMachine(
            referenceImporter,
            nameof(IEnumerator.MoveNext),
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Boolean),
            false,
            baseMoveNextMethod,
            [],
            null,
            stateMachine);

        MethodInfo baseGetCurrentObjectGetMethod = iEnumeratorObjectType
            .GetProperty(nameof(IEnumerator<>.Current))!.GetMethod!;
        MethodDefinition getCurrentObject = AddMethodImplementationToStateMachine(
            referenceImporter,
            $"{iEnumeratorObjectType.Namespace}.{nameof(IEnumerator<>)}<{objectType.FullName}>.get_{nameof(IEnumerator<>.Current)}",
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Object),
            true,
            baseGetCurrentObjectGetMethod,
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

        MethodInfo baseResetMethod = iEnumeratorType.GetMethod(nameof(IEnumerator.Reset))!;
        IMethodDefOrRef notSupportedExceptionCtor = (IMethodDefOrRef)referenceImporter
            .ImportMethod(typeof(NotSupportedException).GetConstructor([])!);
        AddMethodImplementationToStateMachine(
            referenceImporter,
            $"{iEnumeratorType.FullName}.{nameof(IEnumerator.Reset)}",
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Void),
            false,
            baseResetMethod,
            [new(debuggerHiddenAttributeCtor)],
            new()
            {
                Instructions =
                {
                    { Newobj, notSupportedExceptionCtor },
                    Throw
                }
            },
            stateMachine);

        MethodInfo baseGetCurrentGetMethod = iEnumeratorType
            .GetProperty(nameof(IEnumerator.Current))!.GetMethod!;
        MethodDefinition getCurrent = AddMethodImplementationToStateMachine(
            referenceImporter,
            $"{iEnumeratorType.FullName}.get_{nameof(IEnumerator<>.Current)}",
            MethodSignature.CreateInstance(module.CorLibTypeFactory.Object),
            true,
            baseGetCurrentGetMethod,
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
            PropertySignature.CreateInstance(module.CorLibTypeFactory.Object),
            getCurrentObject,
            null,
            stateMachine);

        AddPropertyToStateMachine(
            $"{iEnumeratorType.Namespace}.{nameof(IEnumerator)}.{nameof(IEnumerator.Current)}",
            PropertySignature.CreateInstance(module.CorLibTypeFactory.Object),
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
        ModuleDefinition module,
        ReferenceImporter referenceImporter,
        FieldDefinition stateField,
        IMethodDefOrRef debuggerHiddenAttributeCtor,
        ConstructorInfo objectConstructor,
        TypeDefinition stateMachineType)
    {
        MethodDefinition ctor = MethodDefinition.CreateConstructor(
            module.CorLibTypeFactory,
            [module.CorLibTypeFactory.Int32]);
        ctor.Attributes |= MethodAttributes.HideBySig;
        ctor.CustomAttributes.Add(new(debuggerHiddenAttributeCtor));
        ctor.Parameters[0].GetOrCreateDefinition().Name = stateField.Name;
        ctor.CilMethodBody = new()
        {
            Instructions =
            {
                Ldarg_0,
                { Call, referenceImporter.ImportMethod(objectConstructor) },
                Ldarg_0,
                Ldarg_1,
                { Stfld, stateField },
                Ret
            }
        };
        stateMachineType.Methods.Add(ctor);
    }

    private static MethodDefinition AddMethodImplementationToStateMachine(
        ReferenceImporter referenceImporter,
        string methodName,
        MethodSignature methodSignature,
        bool isSpecialName,
        MethodBase baseMethod,
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
        IMethodDefOrRef baseMethodRef = (IMethodDefOrRef)referenceImporter.ImportMethod(baseMethod);
        // Required so that the IL knows that the method is an override.
        stateMachine.MethodImplementations.Add(new(baseMethodRef, methodDefinition));

        return methodDefinition;
    }
}