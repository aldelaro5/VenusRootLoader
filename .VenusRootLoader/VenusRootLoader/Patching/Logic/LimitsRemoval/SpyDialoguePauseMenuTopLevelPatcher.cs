using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher allows <see cref="EnemyLeaf"/> to have short spy dialogues textboxes be rendered on the PauseMenu.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="PauseMenu.UpdateText"/>: Loosen a game logic that prevented short spy dialogue textboxes to be rendered
/// in the PauseMenu.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class SpyDialoguePauseMenuTopLevelPatcher : ITopLevelPatcher
{
    public void Patch()
    {
        Md.PauseMenu.UpdateText.ILHook(RemoveSpyDialoguesLengthRestriction);
    }

    private static void RemoveSpyDialoguesLengthRestriction(ILManipulationInfo info)
    {
        const int pauseMenuLibraryWindowId = 3;
        ILCursor cursor = new(info.Context);

        // Reaching the library window switch branch.
        ILLabel[] windowSwitchArms = null!;
        cursor.GotoNext(x => x.MatchSwitch(out windowSwitchArms!));
        cursor.GotoLabel(windowSwitchArms[pauseMenuLibraryWindowId]);

        // Skipping over main window render.
        cursor.GotoNext(x => x.MatchSwitch(out _));
        ILLabel afterOptionSwitch = null!;
        cursor.GotoNext(x => x.MatchBranch(out afterOptionSwitch!));
        cursor.GotoLabel(afterOptionSwitch);

        // Reaching the secondoption handling for the bestiary.
        cursor.GotoNext(
            x => x.MatchLdfld<PauseMenu>(nameof(PauseMenu.option)),
            x => x.MatchLdcI4((int)MainManager.Library.Bestiary));

        // Some sanity heuristical control flow naviguation to get closer to the length > 5 check.
        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchBranch(out _));
        cursor.GotoNext(x => x.MatchLdstr("|librarybreak|"));
        cursor.GotoNext(x => x.MatchBranch(out _));

        // We've made it to the > 5 length check.
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Index++;
        cursor.EmitDelegate(ChangeSpyDialogueLengthRead);
    }

    private static int ChangeSpyDialogueLengthRead(int originalLength)
    {
        int enemyGameId = MainManager.listvar[MainManager.instance.option];
        // In practice, the only time in the entire game where the length check causes a change is for this
        // specific enemy since the MothSpyDialogue contains "|next|...|next|". Since the logic is too aggressive
        // for a small change like this, we have to honor what the base game wanted and only let the logic go for this enemy.
        if (enemyGameId == (int)MainManager.Enemies.LeafbugClubber)
            return originalLength;
        // Returning -1 here will mean that the length check will always pass because the game does the array length > -1
        // which will always be true. It only changes the length the game sees on this check, not the actual length.
        return -1;
    }
}