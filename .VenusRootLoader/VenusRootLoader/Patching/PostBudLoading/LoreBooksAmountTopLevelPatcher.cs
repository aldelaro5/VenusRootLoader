using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.PostBudLoading;

/// <summary>
/// This patcher changes map dialogue 7 of AntPalaceLibrary to change the value of a checkvar command which is hardcoded
/// to be the amount of Lore Books in the game. The patched amount reflects the registry.
/// </summary>
internal sealed class LoreBooksAmountTopLevelPatcher : ITopLevelPatcher
{
    private readonly ILeavesRegistry<LoreBookLeaf> _loreBookRegistry;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    public LoreBooksAmountTopLevelPatcher(
        ILeavesRegistry<LoreBookLeaf> loreBookRegistry,
        ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _loreBookRegistry = loreBookRegistry;
        _mapsRegistry = mapsRegistry;
    }

    public void Patch()
    {
        MapLeaf antPalaceLibrary = _mapsRegistry.GetByEffectiveId(nameof(MainManager.Maps.AntPalaceLibrary));
        ILeavesRegistry<MapDialogueLeaf> antPalaceLibraryDialogues = antPalaceLibrary.DialoguesRegistry;
        MapDialogueLeaf dialogueWhenGivingLoreBook = antPalaceLibraryDialogues.GetByGameId(7);

        List<Branch<LanguageLeaf>> allLanguages = dialogueWhenGivingLoreBook.LocalizedText.Keys.ToList();
        foreach (Branch<LanguageLeaf> language in allLanguages)
            PatchLocalizedText(dialogueWhenGivingLoreBook, language);
    }

    private void PatchLocalizedText(MapDialogueLeaf dialogueWhenGivingLoreBook, Branch<LanguageLeaf> language)
    {
        string setTextStringToPatch = dialogueWhenGivingLoreBook.LocalizedText[language];

        string checkVarFlagvar15Prefix = "|checkvar,15,";
        string checkVarFlagVarString = $"{checkVarFlagvar15Prefix}{_loreBookRegistry.CountBaseGame}";
        int indexCheckVarToPatch = setTextStringToPatch.LastIndexOf(
            checkVarFlagVarString,
            StringComparison.InvariantCultureIgnoreCase);
        if (indexCheckVarToPatch == -1)
            return;

        string partBeforeLoreBookAmount = setTextStringToPatch.Substring(
            0,
            indexCheckVarToPatch + checkVarFlagvar15Prefix.Length);
        int amountLoreBooks = _loreBookRegistry.Count;
        string partAfterLoreBookAmount =
            setTextStringToPatch.Substring(indexCheckVarToPatch + checkVarFlagVarString.Length);

        StringBuilder sb = new();
        sb.Append(partBeforeLoreBookAmount);
        sb.Append(amountLoreBooks);
        sb.Append(partAfterLoreBookAmount);

        dialogueWhenGivingLoreBook.LocalizedText[language] = sb.ToString();
        return;
    }
}