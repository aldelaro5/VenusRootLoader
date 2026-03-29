using HarmonyLib;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher adds support for allowing <see cref="MusicLeaf"/> to decide if they should be purchasable from Samira.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.FixSamira"/>: Replaces the entire method to remove the non-purchasable music from our registry.</item>
/// <item><see cref="MainManager.SamiraGotAll"/>: Replaces the entire method to consult the registry to determine if every purchasable music were obtained.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class NonPurchasableMusicsTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    private static NonPurchasableMusicsTopLevelPatcher _instance = null!;

    public NonPurchasableMusicsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _musicRegistry = musicRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(NonPurchasableMusicsTopLevelPatcher));

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.FixSamira))]
    internal static bool FixNonPurchasableMusic()
    {
        if (MainManager.instance.samiramusics is null || MainManager.instance.samiramusics.Count <= 0)
            return false;

        // Going over backwards to avoid indexing problems while removing.
        for (int i = MainManager.instance.samiramusics.Count - 1; i >= 0; i--)
        {
            int gameId = MainManager.instance.samiramusics[i][0];
            if (!_instance._musicRegistry.LeavesByGameIds[gameId].CanBePurchasedFromSamira)
                MainManager.instance.samiramusics.RemoveAt(i);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.SamiraGotAll))]
    internal static bool AdjustPurchasedAllMusic(ref bool __result)
    {
        int amountPurchasableMusic = _instance._musicRegistry.LeavesByGameIds.Values
            .Count(l => l.CanBePurchasedFromSamira);
        __result = MainManager.PurchasedMusicAmmount() >= amountPurchasableMusic;
        return false;
    }
}