using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Logic;

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
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public SpyDialoguePauseMenuTopLevelPatcher(IHarmonyTypePatcher harmonyTypePatcher)
    {
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(SpyDialoguePauseMenuTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.UpdateText))]
    private static IEnumerable<CodeInstruction> RemoveSpyDialoguesLengthRestriction(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        const int pauseMenuLibraryWindowId = 3;
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo optionField = AccessTools.Field(typeof(PauseMenu), nameof(PauseMenu.option));

        // Reaching the library window switch branch.
        matcher.MatchStartForward(Code.Switch);
        Label[] windowSwitchArms = (Label[])matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(windowSwitchArms[pauseMenuLibraryWindowId])));

        // Skipping over main window render.
        matcher.MatchStartForward(Code.Switch);
        matcher.MatchStartForward(CodeMatch.Branches());
        Label afterOptionSwitch = (Label)matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(afterOptionSwitch)));

        // Reaching the secondoption handling for the bestiary.
        matcher.MatchStartForward(
            CodeMatch.LoadsField(optionField),
            CodeMatch.LoadsConstant((int)MainManager.Library.Bestiary));

        // Some sanity heuristical control flow naviguation to get closer to the length > 5 check.
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartForward(CodeMatch.LoadsConstant("|librarybreak|"));
        matcher.MatchStartForward(CodeMatch.Branches());

        // We've made it to the > 5 length check.
        matcher.MatchStartBackwards(CodeMatch.LoadsConstant());
        matcher.Advance(1);
        matcher.Insert(Transpilers.EmitDelegate(ChangeSpyDialogueLengthRead));

        return matcher.Instructions();
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