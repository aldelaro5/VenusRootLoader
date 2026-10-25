using MonoMod.Cil;
using MonoMod.Core.Platforms;
using MonoMod.Utils;
using System.Reflection;

namespace VenusRootLoader.Patching;

/// <summary>
/// An implementation of wrapping an ICall with a DMD to add support to either patch them or invoke their unpatched version.
/// This is directly adapted from the HarmonyX implementation here:
/// https://github.com/BepInEx/HarmonyX/blob/master/Harmony/Public/Patching/NativeDetourMethodPatcher.cs
/// </summary>
internal sealed class DmdICallWrapper
{
    private static readonly ConstructorInfo InvalidOperationExceptionCtor =
        typeof(InvalidOperationException).GetConstructor([typeof(string)])!;

    private SimpleNativeDetour? _detourOriginal;
    private SimpleNativeDetour _detourAltEntry = null!;

    private IntPtr? _originalEntry;

    private MethodBase _replacement = null!;

    private IDisposable? _originalPin;
    private IDisposable? _realReplacementPin;
    private IDisposable? _altHandle;

    private readonly MethodSignature _signature;
    private readonly MethodInfo _altEntryMethod;

    // ReSharper disable once NotAccessedField.Local
    private readonly IDisposable? _altEntryPin;

    private readonly MethodInfo _original;

    public static DmdICallWrapper GetICallWrapper(MethodInfo method) => new(method);

    private DmdICallWrapper(MethodInfo method)
    {
        _original = method;
        _signature = new MethodSignature(method);
        _altEntryMethod = GenerateAltEntryProxyStub(method, _signature);
        _altEntryPin = PlatformTriple.Current.PinMethodIfNeeded(_altEntryMethod);
        PlatformTriple.Current.Compile(_altEntryMethod);
    }

    private static MethodInfo GenerateAltEntryProxyStub(MethodBase original, MethodSignature signature)
    {
        using DynamicMethodDefinition dmd =
            signature.CreateDmd($"NativeDetourAltEntry<{original.DeclaringType?.FullName}:{original.Name}>");

        ILContext il = new(dmd.Definition);
        ILCursor c = new(il);

        c.EmitLdstr($"{dmd.Definition.Name} should've been detoured!");
        c.EmitNewobj(InvalidOperationExceptionCtor);
        c.EmitThrow();

        return dmd.Generate();
    }

    public MethodInfo GetUnpatchedCopy()
    {
        return GenerateAltEntryProxyCaller($"OrigWrapper<{_original.DeclaringType?.FullName}:{_original.Name}>")
            .Generate();
    }

    public MethodBase GenerateWrapper()
    {
        DynamicMethodDefinition callerDmd =
            GenerateAltEntryProxyCaller($"NativeDetourProxy<{_original.DeclaringType?.FullName}:{_original.Name}>");
        MethodBase replacement = callerDmd.Generate();

        PlatformTriple triple = PlatformTriple.Current;

        replacement = triple.GetIdentifiable(replacement);
        if (replacement == _replacement)
            return replacement;

        lock (_altEntryMethod)
        {
            if (replacement == _replacement)
                return replacement;

            _replacement = replacement;

            MethodBase realReplacement = triple.GetRealDetourTarget(_original, replacement);
            _realReplacementPin?.Dispose();
            _realReplacementPin = triple.PinMethodIfNeeded(realReplacement);

            nint realReplacementPtr = triple.Runtime.GetMethodHandle(realReplacement).GetFunctionPointer();

            // SimpleNativeDetour.ChangeTarget with alt entry is not safe currently, refer to docs
            _detourOriginal?.Dispose();
            _altHandle?.Dispose();

            _originalPin ??= triple.PinMethodIfNeeded(_original);
            // due to _originalPin, it shouldn't change
            nint originalEntry = _originalEntry ??= triple.GetNativeMethodBody(_original);

            (_detourOriginal, nint altEntry, _altHandle) = triple.CreateNativeDetour(
                originalEntry,
                realReplacementPtr
            );

            if (_detourAltEntry is { } detourAltEntry)
                detourAltEntry.ChangeTarget(altEntry);
            else
                _detourAltEntry = triple.CreateSimpleDetour(
                    triple.GetNativeMethodBody(_altEntryMethod),
                    altEntry
                );

            return replacement;
        }
    }

    private DynamicMethodDefinition GenerateAltEntryProxyCaller(string name)
    {
        DynamicMethodDefinition dmd = _signature.CreateDmd(name);

        ILContext il = new(dmd.Definition);
        ILCursor c = new(il);

        for (int i = 0; i < _signature.ParameterCount; i++)
            c.EmitLdarg(i);
        c.EmitCall(_altEntryMethod);
        c.EmitRet();

        return dmd;
    }
}