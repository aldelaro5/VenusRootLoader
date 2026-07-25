using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Unity;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public sealed partial class Venus
{
    private bool _hasGlobalBehaviour;

    /// <summary>
    /// Sets a <see cref="GlobalMonoBehaviour"/> to use. Each bud is allowed to have at most 1.
    /// </summary>
    /// <typeparam name="TGlobalMonoBehaviour">The <see cref="GlobalMonoBehaviour"/> type.</typeparam>
    /// <returns>The <see cref="GlobalMonoBehaviour"/> instance that was set.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this method is called more than once for a given bud.</exception>
    public GlobalMonoBehaviour SetGlobalMonoBehaviour<TGlobalMonoBehaviour>()
        where TGlobalMonoBehaviour : GlobalMonoBehaviour
    {
        if (_hasGlobalBehaviour)
            throw new InvalidOperationException($"Cannot call {nameof(SetGlobalMonoBehaviour)} more than once");

        GlobalMonoBehaviour globalMonoBehavior =
            GlobalMonoBehaviourExecution.AddGlobalMonoBehavior<TGlobalMonoBehaviour>(BudId);
        _hasGlobalBehaviour = true;
        return globalMonoBehavior;
    }

    /// <summary>
    /// Obtains a <see cref="Bud"/>'s <see cref="GlobalMonoBehaviour"/>.
    /// </summary>
    /// <param name="budId">The <see cref="Bud"/>'s unique id.</param>
    /// <returns>The <see cref="GlobalMonoBehaviour"/> if it exists or null if it doesn't exist.</returns>
    public GlobalMonoBehaviour? GetGlobalMonoBehaviourFromBud(string budId)
    {
        Guard.IsNotNullOrWhiteSpace(budId);
        return GlobalMonoBehaviourExecution.GetGlobalMonoBehaviourFromGameObject(budId);
    }
}