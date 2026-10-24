using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for custom medal shops including support for base game save loading and a flag to indicate
/// all the stock was bought.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetVariables"/>: Changes the hardcoded medal shops amount so it picks the one from the
/// registry if it exceeds the base game one.</item>
/// <item><see cref="MainManager.Load"/>: Changes the logic that determines how medal shops medals should be loaded,
/// see the comment below for details.</item>
/// <item><see cref="MainManager.EndOfMessage"/>: Updates the bought all flags of medal shops in the registry.</item>
/// <item><see cref="MainManager.SetUpBadges"/>: Defaults the stock of each medal shops to their starting stock in the registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MedalShopsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<MedalShopLeaf> _medalShopsRegistry;

    private static MedalShopsTopLevelPatcher _instance = null!;

    public MedalShopsTopLevelPatcher(ILeavesRegistry<MedalShopLeaf> medalShopsRegistry)
    {
        _instance = this;
        _medalShopsRegistry = medalShopsRegistry;
    }

    public void Patch()
    {
        Md.MainManager.SetVariables.ILHook(PatchBadgeShopsArraysLengthHardCap);
        Md.MainManager.Load.ILHook(PatchAmountBadgeShopsStockLoaded);
        Md.MainManager.EndOfMessage.ILHook(PatchLogicForSettingMedalShopsBoughtAllStockFlags);
        Md.MainManager.SetUpBadges.ControlFlowPrefix(PatchLogicForSettingMedalShopsStartingStock);
    }

    private static void PatchBadgeShopsArraysLengthHardCap(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.badgeshops)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.SetToStaticDelegateCall(PatchNewMedalShopsAmount);
    }

    // This patches Load by changing how loading the medal shop pool lines are treated. It consists of 2 steps for each line:
    // - Overwrites the initializations of the array by all the starting stocks of the shops in the registry
    // - Let the save overwrites the now initialized data with the parts that it has, skipping the ones not in the registry
    // This requires 2 patches each: one to overwrite the initialization and one to overwrite the length read from the save
    // so it skips the shops that aren't in the registry.
    private static void PatchAmountBadgeShopsStockLoaded(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // avaliablebadgepool - overwrites the initialization
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.avaliablebadgepool)));
        while (!cursor.Current.MatchLdloc(out _))
        {
            cursor.Set(OpCodes.Nop);
            cursor.Index--;
        }

        cursor.SetToStaticDelegateCall(InitializeAvailableBadgePoolFromLoad);

        // avaliablebadgepool - overwrite the length so it skips shops that aren't in the registry
        ILLabel forAvailableShopPoolsEndLabel = null!;
        cursor.GotoNext(x => x.MatchBranch(out forAvailableShopPoolsEndLabel!));
        cursor.GotoLabel(forAvailableShopPoolsEndLabel);
        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.GotoPrev(x => x.MatchConvI4());
        cursor.Index++;
        cursor.EmitDelegate(PatchMedalShopsAmountLoadedFromSave);

        // badgeshops - overwrites the initialization
        cursor.GotoNext(x => x.MatchStfld<MainManager>(nameof(MainManager.badgeshops)));
        while (!cursor.Current.MatchLdloc(out _))
        {
            cursor.Set(OpCodes.Nop);
            cursor.Index--;
        }

        cursor.SetToStaticDelegateCall(InitializeBadgesShopsFromLoad);

        // badgeshops - overwrite the length so it skips shops that aren't in the registry
        ILLabel forBadgeShopsEndLabel = null!;
        cursor.GotoNext(x => x.MatchBranch(out forBadgeShopsEndLabel!));
        cursor.GotoLabel(forBadgeShopsEndLabel);
        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.GotoPrev(x => x.MatchConvI4());
        cursor.Index++;
        cursor.EmitDelegate(PatchMedalShopsAmountLoadedFromSave);
    }

    private static void PatchLogicForSettingMedalShopsBoughtAllStockFlags(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo mainManagerInstanceField = typeof(MainManager).FieldByName(nameof(MainManager.instance))!;

        cursor.GotoNext(x => x.MatchLdfld<MainManager>(nameof(MainManager.badgeshops)));
        cursor.GotoPrev(x => x.MatchBranch(out _));

        int cursorIndex = cursor.Index;
        cursor.Index--;
        ILLabel afterProcessingBoughtAllFlagsLabel = null!;
        cursor.GotoPrev(x => x.MatchBranch(out afterProcessingBoughtAllFlagsLabel!));
        cursor.Index = cursorIndex;

        cursor.Set(OpCodes.Br, afterProcessingBoughtAllFlagsLabel);
        cursor.EmitLdsfld(mainManagerInstanceField);
        cursor.EmitDelegate(SetMedalShopsBoughtAllFlags);
    }

    private static ReturnFlow PatchLogicForSettingMedalShopsStartingStock(MainManager instance)
    {
        foreach (MedalShopLeaf medalShopLeaf in _instance._medalShopsRegistry)
        {
            List<int> medalIds = medalShopLeaf.StartingMedalsStock.Select(m => m.Resolve().GameId).ToList();
            instance.badgeshops[medalShopLeaf.GameId].AddRange(medalIds);
            instance.avaliablebadgepool[medalShopLeaf.GameId].AddRange(medalIds);
        }

        return ReturnFlow.SkipOriginal;
    }

    private static int PatchNewMedalShopsAmount() => _instance._medalShopsRegistry.Count;

    // This overwrites the amount of shops read from the save so it skips the ones that don't exist in the registry.
    private static int PatchMedalShopsAmountLoadedFromSave(int lengthFromSave)
    {
        return Math.Min(lengthFromSave, _instance._medalShopsRegistry.Count);
    }

    // This overwrites the initializations of the avaliablebadgepool so they all start with their starting stock before
    // the save overwrites the data it has.
    private static void InitializeAvailableBadgePoolFromLoad(MainManager instance)
    {
        int amountFromRegistry = _instance._medalShopsRegistry.Count;
        instance.avaliablebadgepool = new List<int>[amountFromRegistry];
        for (int i = 0; i < amountFromRegistry; i++)
        {
            instance.avaliablebadgepool[i] = new();
            instance.avaliablebadgepool[i].AddRange(
                _instance._medalShopsRegistry.GetByGameId(i).StartingMedalsStock.Select(m => m.Resolve().GameId));
        }
    }

    // This overwrites the initializations of the badgeshops so they all start with their starting stock before
    // the save overwrites the data it has.
    private static void InitializeBadgesShopsFromLoad(MainManager instance)
    {
        int amountFromRegistry = _instance._medalShopsRegistry.Count;
        instance.badgeshops = new List<int>[amountFromRegistry];
        for (int i = 0; i < amountFromRegistry; i++)
        {
            instance.badgeshops[i] = new();
            instance.badgeshops[i].AddRange(
                _instance._medalShopsRegistry.GetByGameId(i).StartingMedalsStock.Select(m => m.Resolve().GameId));
        }
    }

    private static void SetMedalShopsBoughtAllFlags(MainManager instance)
    {
        foreach (MedalShopLeaf medalShopLeaf in _instance._medalShopsRegistry)
        {
            if (instance.badgeshops[medalShopLeaf.GameId].Count == 0)
                instance.flags[medalShopLeaf.BoughtAllStockFlag.Resolve().GameId] = true;
        }
    }
}