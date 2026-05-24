using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class MedalShopsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MedalShopLeaf> _medalShopsRegistry;

    private static MedalShopsTopLevelPatcher _instance = null!;

    public MedalShopsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MedalShopLeaf> medalShopsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _medalShopsRegistry = medalShopsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(MedalShopsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
    internal static IEnumerable<CodeInstruction> PatchBadgeShopsArraysLengthHardCap(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo mainManagerBadgeShopsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.badgeshops));

        matcher.MatchStartForward(CodeMatch.StoresField(mainManagerBadgeShopsField));
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.SetInstruction(Transpilers.EmitDelegate(PatchNewMedalShopsAmount));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    internal static IEnumerable<CodeInstruction> PatchAmountBadgeShopsStockLoaded(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo mainManagerAvailableBadgePoolField = AccessTools.Field(
            typeof(MainManager),
            nameof(MainManager.avaliablebadgepool));
        FieldInfo mainManagerBadgeShopsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.badgeshops));

        matcher.MatchStartForward(CodeMatch.StoresField(mainManagerAvailableBadgePoolField));
        while (!matcher.Instruction.IsLdloc())
        {
            matcher.SetInstruction(new CodeInstruction(OpCodes.Nop));
            matcher.Advance(-1);
        }

        matcher.SetInstruction(Transpilers.EmitDelegate(InitializeAvailableBadgePoolFromLoad));

        matcher.MatchStartForward(CodeMatch.Branches());
        Label forAvailableShopPoolsEndLabel = (Label)matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(forAvailableShopPoolsEndLabel)));
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartBackwards(Code.Conv_I4);
        matcher.Advance(1);
        matcher.Insert(Transpilers.EmitDelegate(PatchMedalShopsAmountLoadedFromSave));

        matcher.MatchStartForward(CodeMatch.StoresField(mainManagerBadgeShopsField));
        while (!matcher.Instruction.IsLdloc())
        {
            matcher.SetInstruction(new CodeInstruction(OpCodes.Nop));
            matcher.Advance(-1);
        }

        matcher.SetInstruction(Transpilers.EmitDelegate(InitializeBadgesShopsFromLoad));

        matcher.MatchStartForward(CodeMatch.Branches());
        Label forBadgeShopsEndLabel = (Label)matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(forBadgeShopsEndLabel)));
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartBackwards(Code.Conv_I4);
        matcher.Advance(1);
        matcher.Insert(Transpilers.EmitDelegate(PatchMedalShopsAmountLoadedFromSave));

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.EndOfMessage))]
    internal static IEnumerable<CodeInstruction> PatchLogicForSettingMedalShipsBoughtAllStockFlags(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo mainManagerBadgeShopsField = AccessTools.Field(typeof(MainManager), nameof(MainManager.badgeshops));
        FieldInfo mainManagerInstanceField = AccessTools.Field(typeof(MainManager), nameof(MainManager.instance));

        matcher.MatchStartForward(CodeMatch.LoadsField(mainManagerBadgeShopsField));
        matcher.MatchStartBackwards(CodeMatch.Branches());

        CodeMatcher subMatcher = matcher.Clone();
        subMatcher.Advance(-1);
        subMatcher.MatchStartBackwards(CodeMatch.Branches());
        Label afterProcessingBoughtAllFlagsLabel = (Label)subMatcher.Operand;

        matcher.SetInstruction(new CodeInstruction(OpCodes.Br_S, afterProcessingBoughtAllFlagsLabel));
        matcher.Insert(
            new CodeInstruction(OpCodes.Ldsfld, mainManagerInstanceField),
            Transpilers.EmitDelegate(SetMedalShopsBoughtAllFlags));

        return matcher.Instructions();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetUpBadges))]
    internal static bool PatchLogicForSettingMedalShopsStartingStock(MainManager __instance)
    {
        foreach (MedalShopLeaf medalShopLeaf in _instance._medalShopsRegistry.LeavesByGameIds.Values)
        {
            List<int> medalIds = medalShopLeaf.StartingMedalsStock.Select(m => m.GameId).ToList();
            __instance.badgeshops[medalShopLeaf.GameId].AddRange(medalIds);
            __instance.avaliablebadgepool[medalShopLeaf.GameId].AddRange(medalIds);
        }

        return false;
    }

    private static int PatchNewMedalShopsAmount() => _instance._medalShopsRegistry.LeavesByGameIds.Count;

    private static int PatchMedalShopsAmountLoadedFromSave(int lengthFromSave)
    {
        return Math.Min(lengthFromSave, _instance._medalShopsRegistry.LeavesByGameIds.Count);
    }

    private static void InitializeAvailableBadgePoolFromLoad(MainManager instance)
    {
        int amountFromRegistry = _instance._medalShopsRegistry.LeavesByGameIds.Count;
        instance.avaliablebadgepool = new List<int>[amountFromRegistry];
        for (int i = 0; i < amountFromRegistry; i++)
        {
            instance.avaliablebadgepool[i] = new();
            instance.avaliablebadgepool[i].AddRange(
                _instance._medalShopsRegistry.LeavesByGameIds[i].StartingMedalsStock.Select(m => m.GameId));
        }
    }

    private static void InitializeBadgesShopsFromLoad(MainManager instance)
    {
        int amountFromRegistry = _instance._medalShopsRegistry.LeavesByGameIds.Count;
        instance.badgeshops = new List<int>[amountFromRegistry];
        for (int i = 0; i < amountFromRegistry; i++)
        {
            instance.badgeshops[i] = new();
            instance.badgeshops[i].AddRange(
                _instance._medalShopsRegistry.LeavesByGameIds[i].StartingMedalsStock.Select(m => m.GameId));
        }
    }

    private static void SetMedalShopsBoughtAllFlags(MainManager instance)
    {
        foreach (MedalShopLeaf medalShopLeaf in _instance._medalShopsRegistry.LeavesByGameIds.Values)
        {
            if (instance.badgeshops[medalShopLeaf.GameId].Count == 0)
                instance.flags[medalShopLeaf.BoughtAllStockFlag.GameId] = true;
        }
    }
}