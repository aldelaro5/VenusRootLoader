using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Patching;

namespace VenusRootLoader.Unity;

public interface IGlobalMonoBehaviourExecution
{
    GlobalMonoBehaviour AddGlobalMonoBehavior<T>(string gameObjectName)
        where T : GlobalMonoBehaviour;

    GlobalMonoBehaviour? GetGlobalMonoBehaviourFromGameObject(string gameObjectName);
}

internal sealed class GlobalMonoBehaviourExecution : IGlobalMonoBehaviourExecution
{
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