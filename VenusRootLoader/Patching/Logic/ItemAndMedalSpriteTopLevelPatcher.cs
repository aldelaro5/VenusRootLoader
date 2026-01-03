using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class ItemAndMedalSpriteTopLevelPatcher : ITopLevelPatcher
{
    private enum SpriteKind
    {
        Item,
        Medal
    }

    private static ItemAndMedalSpriteTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<ItemLeaf, int> _itemLeafRegistry;

    private readonly Dictionary<int, Sprite> _customMedalSprites = new();

    public ItemAndMedalSpriteTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<ItemLeaf, int> itemLeafRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _itemLeafRegistry = itemLeafRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ItemAndMedalSpriteTopLevelPatcher));

    internal void AssignMedalSprite(int itemId, Sprite sprite) => _customMedalSprites[itemId] = sprite;

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.LoadItemSprites))]
    private static IEnumerable<CodeInstruction> InsertCustomItemSpriteHandler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        CodeMatcher codeMatcher = new(instructions, generator);
        // We first need to patch a hardcoded cap where at most, 256 items or medals can exist
        codeMatcher.MatchEndForward(new CodeMatch(OpCodes.Ldc_I4, 256));
        codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<int>>(PatchItemOrMedalsSpritesCap));

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

        // Get to the inner loop where medals and items are processed individually
        codeMatcher.MatchEndForward(CodeMatch.Branches());

        // We need to take a detour to get the position where the inner loop increments and
        // advances to the next iteration. This will be used in the hooks later
        CodeMatcher codeMatcherInnerLoop = codeMatcher.Clone();
        Label labelInnerLoopEnd = (Label)codeMatcherInnerLoop.Operand;
        // This is the end of the loop, but we need to go backward to get to the increment part
        codeMatcherInnerLoop.SearchForward(inst => inst.labels.Contains(labelInnerLoopEnd));
        codeMatcherInnerLoop.Advance(-1);
        codeMatcherInnerLoop.MatchStartBackwards(CodeMatch.IsLdloc());
        int positionInnerLoopIncrement = codeMatcherInnerLoop.Pos;
        // We can now jump to the increment after our hooks
        codeMatcher.CreateLabelAt(positionInnerLoopIncrement, out Label labelInnerLoopIncrement);

        // Back to the main patch, we need to get to the part where the type of sprites is decided in the inner loop
        codeMatcher.Advance(1);
        codeMatcher.MatchEndForward(CodeMatch.Branches());

        // We need to follow this label later to get to our item hook insertion point, but we also need to change it after
        // because we are inserting code before its destination which requires a new label we define
        Label labelBeforeSetItemSprite = (Label)codeMatcher.Operand;
        codeMatcher.DefineLabel(out Label labelItemHook);
        codeMatcher.SetOperandAndAdvance(labelItemHook);

        // We have reached the medals hook insertion point
        codeMatcher.InsertAndAdvance(Code.Ldc_I4_1);
        codeMatcher.InsertAndAdvance(Code.Ldloc_3);
        codeMatcher.InsertAndAdvance(
            Transpilers.EmitDelegate<Func<SpriteKind, int, bool>>(PatchCustomItemOrMedalSprite));
        // This will skip the loop iteration if our hook handled the sprite assignment using the label we got earlier
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue_S, labelInnerLoopIncrement));

        // Gets us to the item hook insertion point
        codeMatcher.SearchForward(inst => inst.labels.Contains(labelBeforeSetItemSprite));
        codeMatcher.InsertAndAdvance(new CodeInstruction(Code.Ldc_I4_0) { labels = [labelItemHook] });
        codeMatcher.InsertAndAdvance(Code.Ldloc_3);
        codeMatcher.InsertAndAdvance(
            Transpilers.EmitDelegate<Func<SpriteKind, int, bool>>(PatchCustomItemOrMedalSprite));
        // This will skip the loop iteration if our hook handled the sprite assignment using the label we got earlier
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue_S, labelInnerLoopIncrement));

        return codeMatcher.Instructions();
    }

    private static int PatchItemOrMedalsSpritesCap() => Math.Max(
        Enum.GetNextIntEnumValue(typeof(MainManager.Items)),
        Enum.GetNextIntEnumValue(typeof(MainManager.BadgeTypes)));

    private static bool PatchCustomItemOrMedalSprite(SpriteKind type, int itemId)
    {
        Sprite? itemSprite = null;
        Sprite? medalSprite = null;

        if (type == SpriteKind.Item)
        {
            ItemLeaf? itemLeaf = _instance._itemLeafRegistry.Leaves.Values.FirstOrDefault(l => l.GameId == itemId);
            if (itemLeaf is null)
                return false;
            itemSprite = itemLeaf.Sprite;
        }

        if (type == SpriteKind.Medal && !_instance._customMedalSprites.TryGetValue(itemId, out medalSprite))
            return false;

        MainManager.itemsprites[(int)type, itemId] = type == SpriteKind.Item ? itemSprite! : medalSprite!;
        return true;
    }
}