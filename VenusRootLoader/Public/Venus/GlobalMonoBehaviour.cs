using CommunityToolkit.Diagnostics;

// ReSharper disable CheckNamespace

namespace VenusRootLoader.Public;

public sealed partial class Venus
{
    public GlobalMonoBehaviour SetGlobalMonoBehaviour<T>()
        where T : GlobalMonoBehaviour
    {
        if (_hasGlobalBehaviour)
            throw new InvalidOperationException($"Cannot call {nameof(SetGlobalMonoBehaviour)} more than once");

        GlobalMonoBehaviour globalMonoBehavior =
            _venusServices.GlobalMonoBehaviourExecution.AddGlobalMonoBehavior<T>(_budId);
        _hasGlobalBehaviour = true;
        return globalMonoBehavior;
    }

    public GlobalMonoBehaviour? GetGlobalMonoBehaviourFromBud(string budId)
    {
        Guard.IsNotNullOrWhiteSpace(budId);
        return _venusServices.GlobalMonoBehaviourExecution.GetGlobalMonoBehaviourFromGameObject(budId);
    }
}