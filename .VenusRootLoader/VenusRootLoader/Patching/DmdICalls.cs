using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching;

/// <summary>
/// All the <see cref="DmdICall"/>s used for patching.
/// </summary>
internal static class DmdICalls
{
    /// <summary>
    /// <see cref="DmdICall"/> for <see cref="UnityEngine.Resources.Load(string, Type)"/>.
    /// </summary>
    internal static DmdICall UnityEngineResourcesLoad { get; } = new(
        typeof(UnityEngine.Resources).MethodByName(
            nameof(UnityEngine.Resources.Load),
            [typeof(string), typeof(Type)])!);

    /// <summary>
    /// <see cref="DmdICall"/> for <see cref="UnityEngine.Resources.LoadAll(string, Type)"/>.
    /// </summary>
    internal static DmdICall UnityEngineResourcesLoadAll { get; } = new(
        typeof(UnityEngine.Resources).MethodByName(
            nameof(UnityEngine.Resources.LoadAll),
            [typeof(string), typeof(Type)])!);
}