using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Unity;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private bool _hasGlobalBehaviour;

    public GlobalMonoBehaviour SetGlobalMonoBehaviour<T>()
        where T : GlobalMonoBehaviour
    {
        if (_hasGlobalBehaviour)
            throw new InvalidOperationException($"Cannot call {nameof(SetGlobalMonoBehaviour)} more than once");

        GlobalMonoBehaviour globalMonoBehavior = _globalMonoBehaviourExecution.AddGlobalMonoBehavior<T>(_budId);
        _hasGlobalBehaviour = true;
        return globalMonoBehavior;
    }

    public GlobalMonoBehaviour? GetGlobalMonoBehaviourFromBud(string budId)
    {
        Guard.IsNotNullOrWhiteSpace(budId);
        return _globalMonoBehaviourExecution.GetGlobalMonoBehaviourFromGameObject(budId);
    }
}