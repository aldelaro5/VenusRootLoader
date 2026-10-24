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
        _dmdICallWrapper = DmdICallWrapper.GetICallWrapper(method);
        Unpatched = _dmdICallWrapper.GetUnpatchedCopy();
        PatchedWrapper = _dmdICallWrapper.GenerateWrapper();
    }
}