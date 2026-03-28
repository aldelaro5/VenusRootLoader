using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Unity;

/// <summary>
/// A service offered to buds that allows them to register <see cref="GlobalMonoBehaviour"/>
/// which are special <see cref="MonoBehaviour"/> in the sense that <see cref="VenusRootLoader"/> will ensure
/// that they have the highest possible priority over the game.
/// Their relative priority are determined by the buds's load order and are primarily intended for a bud to
/// globally handle Unity events.
/// </summary>
internal interface IGlobalMonoBehaviourExecution
{
    /// <summary>
    /// Adds a <see cref="GlobalMonoBehaviour"/> to a newly created <see cref="GameObject"/> with the following properties:
    /// <list type="bullet">
    /// <item>Won't belong to any scenes.</item>
    /// <item>Will be marked <see cref="UnityEngine.Object.DontDestroyOnLoad"/>.</item>
    /// <item>Will be marked <see cref="HideFlags.HideAndDontSave"/>.</item>
    /// </list>
    /// </summary>
    /// <param name="gameObjectName">The name of the <see cref="GameObject"/> that will contain the <see cref="GlobalMonoBehaviour"/></param>
    /// <typeparam name="TGlobalMonoBehaviour">The type of the <see cref="GlobalMonoBehaviour"/></typeparam>
    /// <returns>The added <see cref="GlobalMonoBehaviour"/> instance</returns>
    GlobalMonoBehaviour AddGlobalMonoBehavior<TGlobalMonoBehaviour>(string gameObjectName)
        where TGlobalMonoBehaviour : GlobalMonoBehaviour;

    /// <summary>
    /// Obtains a <see cref="GlobalMonoBehaviour"/> using its <see cref="GameObject"/>'s name.
    /// </summary>
    /// <param name="gameObjectName">The <see cref="GameObject"/> name to check for a <see cref="GlobalMonoBehaviour"/>.</param>
    /// <returns>The <see cref="GlobalMonoBehaviour"/> if it exists or null if it doesn't exist.</returns>
    GlobalMonoBehaviour? GetGlobalMonoBehaviourFromGameObject(string gameObjectName);
}

/// <inheritdoc/>
internal sealed class GlobalMonoBehaviourExecution : IGlobalMonoBehaviourExecution
{
    // The game's highest priority MonoBehaviour is InputIOManager.InputIO which is set to -200
    // so we want to stop at -201 should we ever reach this.
    private const int LastAvailableExecutionOrder = -201;

    private readonly GameObject _globalGameObject = new(nameof(VenusRootLoader));
    private int _nextExecutionOrder = int.MinValue;
    private static Dictionary<Type, int> CustomExecutionOrders { get; } = new();

    public GlobalMonoBehaviourExecution(IHarmonyTypePatcher harmonyTypePatcher)
    {
        harmonyTypePatcher.PatchAll(typeof(GlobalMonoBehaviourExecution));
        UnityEngine.Object.DontDestroyOnLoad(_globalGameObject);
        _globalGameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    public GlobalMonoBehaviour AddGlobalMonoBehavior<T>(string gameObjectName)
        where T : GlobalMonoBehaviour
    {
        CustomExecutionOrders.Add(typeof(T), _nextExecutionOrder);
        if (_nextExecutionOrder != LastAvailableExecutionOrder)
            _nextExecutionOrder++;

        GameObject gameObject = new(gameObjectName);
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        gameObject.transform.SetParent(_globalGameObject.transform);
        return gameObject.AddComponent<T>();
    }

    public GlobalMonoBehaviour? GetGlobalMonoBehaviourFromGameObject(string gameObjectName)
    {
        Transform? transform = _globalGameObject.transform.Find(gameObjectName);
        return transform?.GetComponent<GlobalMonoBehaviour>();
    }

    // This method is how Unity allows to put a DefaultExecutionOrder attribute to control the number at runtime
    // instead of serializing it from the editor. By using a false returning prefix, we can effectively make Unity act
    // as if we just happened to put that attribute ourselves when in reality, we determine the value at runtime.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AttributeHelperEngine), nameof(AttributeHelperEngine.GetDefaultExecutionOrderFor))]
    private static bool ChangeExecutionOrderForGlobalMonoBehaviours(Type klass, ref int __result)
    {
        if (!CustomExecutionOrders.TryGetValue(klass, out int value))
            return true;

        __result = value;
        return false;
    }
}