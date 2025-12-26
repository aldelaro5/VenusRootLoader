using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using UnityEngine;

namespace VenusRootLoader.Patching;

internal sealed class ItemSpritePatcher
{
    private static ItemSpritePatcher _instance = null!;

    private readonly ILogger<ItemSpritePatcher> _logger;

    private readonly Dictionary<int, Sprite> _customItemSprites = new();

    public ItemSpritePatcher(IHarmonyTypePatcher harmonyTypePatcher, ILogger<ItemSpritePatcher> logger)
    {
        _instance = this;
        _logger = logger;
        harmonyTypePatcher.PatchAll(typeof(ItemSpritePatcher));
    }

    internal void AssignItemSprite(int itemId, Sprite sprite)
    {
        _customItemSprites[itemId] = sprite;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.LoadItemSprites))]
    private static IEnumerable<CodeInstruction> InsertCustomItemSpriteHandler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher codeMatcher = new(instructions, generator);
        // A loop that has 2 iterations (0 = items, 1 = medals)
        codeMatcher.MatchEndForward(CodeMatch.Branches());
        codeMatcher.Advance(1);

        // Brings us to the logic for getting the items count
        codeMatcher.MatchEndForward(CodeMatch.Branches());
        Label labelItemCounting = (Label)codeMatcher.Operand;
        codeMatcher.SearchForward(inst => inst.labels.Contains(labelItemCounting));

        // Patch a game bug where it incorrectly assumes the amount of Items enum values are the amount of items
        // in the game, but this isn't the case because of the presence of -1 for None so it's off by 1
        codeMatcher.MatchEndForward(Code.Ldlen);
        codeMatcher.InsertAfterAndAdvance(Code.Ldc_I4_1);
        codeMatcher.InsertAfterAndAdvance(Code.Sub);

        // Get to the items loop
        codeMatcher.MatchEndForward(CodeMatch.Branches());

        // We need to take a detour to get the position where the items loop increments and
        // advances to the next iteration. This will be used in the hook later
        CodeMatcher codeMatcherInnerLoop = codeMatcher.Clone();
        Label labelInnerLoopEnd = (Label)codeMatcherInnerLoop.Operand;
        // This is the end of the loop, but we need to go backward to get to the increment part
        codeMatcherInnerLoop.SearchForward(inst => inst.labels.Contains(labelInnerLoopEnd));
        codeMatcherInnerLoop.Advance(-1);
        codeMatcherInnerLoop.MatchStartBackwards(CodeMatch.IsLdloc());
        int positionInnerLoopIncrement = codeMatcherInnerLoop.Pos;

        // Back to the main patch, we need to get to the part where items are processed
        codeMatcher.Advance(1);
        codeMatcher.MatchEndForward(CodeMatch.Branches());
        // We need to follow this label to get to our hook insertion point, but we also need to change it after
        // because we are inserting code before its destination which requires a new label we define
        Label labelBeforeSetItemSprite = (Label)codeMatcher.Operand;
        codeMatcher.DefineLabel(out Label labelHook);
        codeMatcher.SetOperandAndAdvance(labelHook);
        codeMatcher.SearchForward(inst => inst.labels.Contains(labelBeforeSetItemSprite));

        // We have finally reached our hook insertion point
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3) { labels = [labelHook] });
        codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(PatchCustomItemSprite));
        // This will skip the loop iteration if our hook handled the sprite assignment using the position we found earlier
        codeMatcher.InsertBranchAndAdvance(OpCodes.Brtrue_S, positionInnerLoopIncrement);

        return codeMatcher.Instructions();
    }

    private static bool PatchCustomItemSprite(int itemId)
    {
        if (!_instance._customItemSprites.TryGetValue(itemId, out Sprite sprite))
            return false;

        MainManager.itemsprites[0, itemId] = sprite;
        _instance._logger.LogTrace(itemId.ToString());
        return true;
    }
}