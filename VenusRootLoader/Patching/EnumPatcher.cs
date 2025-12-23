using HarmonyLib;
using System.Reflection;

namespace VenusRootLoader.Patching;

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
            _customEnumNames[type] = new((ulong)Enum.GetValues(type).Cast<int>().Max() + 1);
        _customEnumNames[type].CustomNames.Add(name);

        int resultId = (int)_customEnumNames[type].NextId;
        _customEnumNames[type].NextId++;
        _runtimeTypeGenericCacheField.SetValue(type, null);
        return resultId;
    }

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