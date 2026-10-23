using MonoDetour.DetourTypes;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for allowing <see cref="MusicLeaf"/> to decide if they should be purchasable from Samira.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.FixSamira"/>: Replaces the entire method to remove the non-purchasable music from our registry.</item>
/// <item><see cref="MainManager.SamiraGotAll"/>: Replaces the entire method to consult the registry to determine if
/// every purchasable music were obtained.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class NonPurchasableMusicsTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    private static NonPurchasableMusicsTopLevelPatcher _instance = null!;

    public NonPurchasableMusicsTopLevelPatcher(ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        _instance = this;
        _musicRegistry = musicRegistry;
    }

    public void Patch()
    {
        Md.MainManager.FixSamira.ControlFlowPrefix(FixNonPurchasableMusic);
        Md.MainManager.SamiraGotAll.ControlFlowPrefix(AdjustPurchasedAllMusic);
    }

    private static ReturnFlow FixNonPurchasableMusic()
    {
        if (MainManager.instance.samiramusics is null || MainManager.instance.samiramusics.Count <= 0)
            return ReturnFlow.SkipOriginal;

        // Going over backwards to avoid indexing problems while removing.
        for (int i = MainManager.instance.samiramusics.Count - 1; i >= 0; i--)
        {
            int gameId = MainManager.instance.samiramusics[i][0];
            if (!_instance._musicRegistry.GetByGameId(gameId).CanBePurchasedFromSamira)
                MainManager.instance.samiramusics.RemoveAt(i);
        }

        return ReturnFlow.SkipOriginal;
    }

    private static ReturnFlow AdjustPurchasedAllMusic(ref bool result)
    {
        int amountPurchasableMusic = _instance._musicRegistry.Count(l => l.CanBePurchasedFromSamira);
        result = MainManager.PurchasedMusicAmmount() >= amountPurchasableMusic;
        return ReturnFlow.SkipOriginal;
    }
}