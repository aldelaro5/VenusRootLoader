using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher adds support for custom obtained flags from the <see cref="LoreBookLeaf"/> and <see cref="MedalFortuneTellerHintLeaf"/>
/// to exist in the game such that they work properly for the fortune-teller hints.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.Event71"/>: Changes the hardcoded obtained flags arrays used for Lore Books and Medal instances.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class FortuneTellerHintFlagsTopLevelPatcher : ITopLevelPatcher
{
    private static FortuneTellerHintFlagsTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<LoreBookLeaf> _loreBooksRegistry;
    private readonly ILeavesRegistry<MedalFortuneTellerHintLeaf> _medalFortuneTellerHintsRegistry;

    public FortuneTellerHintFlagsTopLevelPatcher(
        ILeavesRegistry<LoreBookLeaf> loreBooksRegistry,
        ILeavesRegistry<MedalFortuneTellerHintLeaf> medalFortuneTellerHintsRegistry)
    {
        _instance = this;
        _loreBooksRegistry = loreBooksRegistry;
        _medalFortuneTellerHintsRegistry = medalFortuneTellerHintsRegistry;
    }

    public void Patch()
    {
        Md.EventControl.Event71.ILHookMoveNext(PatchFortuneTellerHintFlags);
    }

    private static void PatchFortuneTellerHintFlags(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo flagsField = info.Original!.DeclaringType
            .GetRuntimeFields()
            .Single(f => f.Name.Contains("<flags>"));

        cursor.GotoNext(x => x.MatchCallOrCallvirt<UnityEngine.Resources>(nameof(UnityEngine.Resources.Load)));
        cursor.GotoNext(x => x.MatchStfld(flagsField));
        cursor.EmitDelegate(GetNewFortuneTallerHintFlagsArray);
    }

    private static int[][] GetNewFortuneTallerHintFlagsArray(int[][] original) =>
    [
        _instance._loreBooksRegistry
            .Select(l => l.LoreBookObtainedFlag.Resolve().GameId)
            .ToArray(),
        _instance._medalFortuneTellerHintsRegistry
            .Select(l => l.MedalObtainedFlag.Resolve().GameId)
            .ToArray()
    ];
}