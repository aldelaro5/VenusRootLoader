using MonoMod;
using System.Reflection;

namespace VenusRootLoader.Patching;

/// <summary>
/// Exposes the unpatched and patched versions of an ICall obtained from a <see cref="DmdICallWrapper"/>.
/// </summary>
internal sealed class DmdICall
{
    private readonly DmdICallWrapper _dmdICallWrapper;

    /// <summary>
    /// The unpatched version of the ICall.
    /// </summary>
    internal MethodBase Unpatched { get; }

    /// <summary>
    /// The patched version of the ICall.
    /// </summary>
    internal MethodBase PatchedWrapper { get; }

    public DmdICall(MethodInfo method)
    {
        // We have to temporarily use the MethodBuilder DMD backend for this because DynamicMethod doesn't work for hooking
        // due to MonoMod being unable to get their method bodies.
        bool hadValue = Switches.TryGetSwitchValue(nameof(Switches.DMDType), out object? dmdType);
        Switches.SetSwitchValue(nameof(Switches.DMDType), "mb");

        _dmdICallWrapper = DmdICallWrapper.GetICallWrapper(method);
        Unpatched = _dmdICallWrapper.GetUnpatchedCopy();
        PatchedWrapper = _dmdICallWrapper.GenerateWrapper();

        if (hadValue)
            Switches.SetSwitchValue(nameof(Switches.DMDType), dmdType);
        else
            Switches.ClearSwitchValue(nameof(Switches.DMDType));
    }
}