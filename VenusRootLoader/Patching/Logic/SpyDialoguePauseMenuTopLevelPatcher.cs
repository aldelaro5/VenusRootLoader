using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace VenusRootLoader.Patching.Logic;

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

        matcher.MatchStartForward(Code.Switch);
        Label[] windowSwitchArms = (Label[])matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(windowSwitchArms[pauseMenuLibraryWindowId])));
        matcher.MatchStartForward(Code.Switch);
        matcher.MatchStartForward(CodeMatch.Branches());
        Label afterOptionSwitch = (Label)matcher.Operand;
        matcher.MatchStartForward(new CodeMatch(i => i.labels.Contains(afterOptionSwitch)));
        matcher.MatchStartForward(
            CodeMatch.LoadsField(optionField),
            CodeMatch.LoadsConstant((int)MainManager.Library.Bestiary));
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.Advance(1);
        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.MatchStartForward(CodeMatch.LoadsConstant("|librarybreak|"));
        matcher.MatchStartForward(CodeMatch.Branches());
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
        // for a small change like this, we have to honor what BaseGame wanted and only let the logic go for this enemy
        if (enemyGameId == (int)MainManager.Enemies.LeafbugClubber)
            return originalLength;
        return -1;
    }
}