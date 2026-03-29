using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for custom obtained flags from the <see cref="LoreBookLeaf"/> and <see cref="MedalFortuneTellerHintLeaf"/>
/// to exist in the game such that they work properly for the fortune-teller hints.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.Event71"/>: Changes the hardcoded obtained flags arrays used for Lore Books and Medal instances.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class FortuneTellerHintFlagsTopLevelPatcher : ITopLevelPatcher
{
    private static FortuneTellerHintFlagsTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<LoreBookLeaf> _loreBooksRegistry;
    private readonly ILeavesRegistry<MedalFortuneTellerHintLeaf> _medalFortuneTellerHintsRegistry;

    public FortuneTellerHintFlagsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<LoreBookLeaf> loreBooksRegistry,
        ILeavesRegistry<MedalFortuneTellerHintLeaf> medalFortuneTellerHintsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _loreBooksRegistry = loreBooksRegistry;
        _medalFortuneTellerHintsRegistry = medalFortuneTellerHintsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(FortuneTellerHintFlagsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EventControl), nameof(EventControl.Event71), MethodType.Enumerator)]
    internal static IEnumerable<CodeInstruction> PatchFortuneTellerHintFlags(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo resourcesLoadTextAssetMethod = AccessTools.GetDeclaredMethods(typeof(UnityEngine.Resources))
            .Single(m => m.Name == nameof(UnityEngine.Resources.Load) && m.ContainsGenericParameters)
            .MakeGenericMethod(typeof(TextAsset));
        FieldInfo flagsField = method.DeclaringType
            .GetRuntimeFields()
            .Single(f => f.Name.Contains("<flags>"));

        matcher.MatchStartForward(CodeMatch.Calls(resourcesLoadTextAssetMethod));
        matcher.MatchStartForward(CodeMatch.StoresField(flagsField));
        matcher.Insert(Transpilers.EmitDelegate(GetNewFortuneTallerHintFlagsArray));

        return matcher.Instructions();
    }

    private static int[][] GetNewFortuneTallerHintFlagsArray(int[][] original) =>
    [
        _instance._loreBooksRegistry.LeavesByGameIds.Values
            .Select(l => l.LoreBookObtainedFlag.GameId)
            .ToArray(),
        _instance._medalFortuneTellerHintsRegistry.LeavesByGameIds.Values
            .Select(l => l.MedalObtainedFlag.GameId)
            .ToArray()
    ];
}