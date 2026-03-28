using HarmonyLib;
using System.Reflection;
using VenusRootLoader.Extensions;

namespace VenusRootLoader.Patching;

/// <summary>
/// A patching service that allows to add custom enum values at runtime by patching Mono's mscorlib implementation.
/// </summary>
internal sealed class EnumPatcher
{
    private class CustomEnumNamesInfo
    {
        internal readonly ulong FirstFreeId;
        internal ulong NextId;
        internal List<string> CustomNames { get; } = [];

        internal CustomEnumNamesInfo(ulong firstFreeId)
        {
            FirstFreeId = firstFreeId;
            NextId = firstFreeId;
        }
    }

    private static EnumPatcher _instance = null!;

    private readonly Dictionary<Type, CustomEnumNamesInfo> _customEnumNames = new();

    // A field that when set to null, it clears the enum values cache of the type and will force Mono to go through our
    // patch below to fetch the new enum values.
    private readonly FieldInfo _runtimeTypeGenericCacheField =
        AccessTools.TypeByName("System.RuntimeType").Field("GenericCache");

    public EnumPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _instance = this;
        harmonyTypePatcher.PatchAll(typeof(EnumPatcher));
    }

    internal int AddCustomEnumName(Type type, string name)
    {
        if (!_customEnumNames.ContainsKey(type))
            _customEnumNames[type] = new((ulong)Enum.GetNextIntEnumValue(type));
        if (_customEnumNames[type].CustomNames.Contains(name))
            throw new ArgumentException($"The custom enum name {name} already exists in {type.FullDescription()}");

        _customEnumNames[type].CustomNames.Add(name);

        int resultId = (int)_customEnumNames[type].NextId;
        _customEnumNames[type].NextId++;
        _runtimeTypeGenericCacheField.SetValue(type, null);
        return resultId;
    }

    // This method is an icall from Mono's mscorlib that fetches a list of values and names in 2 separate arrays that matches the
    // ones available for the enum types. Notably, this method is ALWAYS involved somehow in any of the System.Enum APIs
    // meaning that just by patching this, we essentially guarantee that as far as Mono and the game is concerned, if we
    // add values to those list, they simply exist from now on. The only thing to keep in mind is this is only true if the
    // internal cache of the enum type hasn't been filled yet. This means we need to clear the cache on any changes done
    // through this patch.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enum), "GetEnumValuesAndNames")]
    private static void AddCustomEnumValuesAndNames(
        object enumType,
        ref ulong[] values,
        ref string[] names)
    {
        Type type = (Type)enumType;
        if (!_instance._customEnumNames.TryGetValue(type, out CustomEnumNamesInfo customEnumNamesInfo))
            return;

        List<string> newNames = names.ToList();
        newNames.AddRange(customEnumNamesInfo.CustomNames);
        names = newNames.ToArray();

        List<ulong> newValues = values.ToList();
        for (ulong i = customEnumNamesInfo.FirstFreeId; i < customEnumNamesInfo.NextId; i++)
            newValues.Add(i);

        values = newValues.ToArray();
    }
}