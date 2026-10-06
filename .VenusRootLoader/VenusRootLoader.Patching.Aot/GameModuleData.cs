using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VenusRootLoader.Patching.Aot;

public sealed class GameModuleData
{
    private readonly ReferenceImporter _referenceImporter;
    public ModuleDefinition Module { get; }

    public ITypeDefOrRef TypeType { get; }
    public IMethodDescriptor ObjectConstructorMethod { get; }
    public IMethodDescriptor NotSupportedExceptionCtor { get; }
    public IMethodDescriptor DebuggerHiddenAttributeCtor { get; }
    public IMethodDescriptor CompilerGeneratedAttributeCotr { get; }
    public IMethodDescriptor IteratorStateMachineAttributeCtor { get; }
    public ITypeDefOrRef EnumeratorType { get; }
    public IMethodDescriptor EnumeratorGetCurrent { get; }
    public IMethodDescriptor EnumeratorMoveNext { get; }
    public IMethodDescriptor EnumeratorReset { get; }
    public ITypeDefOrRef EnumeratorObjectType { get; }
    public IMethodDescriptor EnumeratorObjectGetCurrent { get; }
    public ITypeDefOrRef DisposableType { get; }
    public IMethodDescriptor DisposableDispose { get; }

    public AssemblyReference UnityCoreModuleReference { get; }
    public ITypeDefOrRef CoroutineType { get; }
    public IMethodDescriptor StartCoroutineMethod { get; }

    public FieldDefinition MainManagerInstance { get; }

    public GameModuleData(ModuleDefinition module, ReferenceImporter referenceImporter)
    {
        _referenceImporter = referenceImporter;
        Module = module;

        Type iDisposable = typeof(IDisposable);
        Type iEnumerator = typeof(IEnumerator);
        Type iEnumeratorObject = typeof(IEnumerator<object>);
        Type type = typeof(Type);

        TypeType = referenceImporter.ImportType(type);
        ObjectConstructorMethod = referenceImporter.ImportMethod(typeof(object).GetConstructor([])!);
        DebuggerHiddenAttributeCtor =
            referenceImporter.ImportMethod(typeof(DebuggerHiddenAttribute).GetConstructor([])!);
        NotSupportedExceptionCtor = referenceImporter.ImportMethod(typeof(NotSupportedException).GetConstructor([])!);
        CompilerGeneratedAttributeCotr =
            referenceImporter.ImportMethod(typeof(CompilerGeneratedAttribute).GetConstructor([])!);
        IteratorStateMachineAttributeCtor =
            referenceImporter.ImportMethod(typeof(IteratorStateMachineAttribute).GetConstructor([type])!);

        EnumeratorType = referenceImporter.ImportType(iEnumerator);
        EnumeratorGetCurrent =
            referenceImporter.ImportMethod(iEnumerator.GetProperty(nameof(IEnumerator.Current))!.GetMethod!);
        EnumeratorMoveNext = referenceImporter.ImportMethod(iEnumerator.GetMethod(nameof(IEnumerator.MoveNext))!);
        EnumeratorReset = referenceImporter.ImportMethod(iEnumerator.GetMethod(nameof(IEnumerator.Reset))!);

        EnumeratorObjectType = referenceImporter.ImportType(typeof(IEnumerator<object>));
        EnumeratorObjectGetCurrent = referenceImporter.ImportMethod(
            iEnumeratorObject
                .GetProperty(nameof(IEnumerator<>.Current))!.GetMethod!);

        DisposableType = referenceImporter.ImportType(iDisposable);
        DisposableDispose =
            referenceImporter.ImportMethod(iDisposable.GetMethod(nameof(IDisposable.Dispose))!);

        MainManagerInstance = module
            .GetAllTypes()
            .Single(x => x.Name == "MainManager")
            .Fields.Single(x => x.Name == "instance");

        UnityCoreModuleReference = module.AssemblyReferences
            .Single(x => x.Name == "UnityEngine.CoreModule");
        CoroutineType = UnityCoreModuleReference
            .CreateTypeReference("UnityEngine", "Coroutine");
        MethodSignature methodSignature = MethodSignature.CreateInstance(
            CoroutineType.ToTypeSignature(false),
            [EnumeratorType.ToTypeSignature(false)]);
        StartCoroutineMethod = UnityCoreModuleReference
            .CreateTypeReference("UnityEngine", "MonoBehaviour")
            .CreateMethodReference("StartCoroutine", methodSignature);
    }
}