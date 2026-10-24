using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Extensions;

namespace VenusRootLoader.Patching.Logic.LimitsRemoval;

/// <summary>
/// This patcher changes the initialization of some array fields of <see cref="NPCControl"/> such that they default to
/// have a length of 0 instead of 1. This allows previously awkward or impossible combinations such as an
/// enemy having no items drops without having any data elements.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="NPCControl()"/>: Changes the default length of <see cref="NPCControl.data"/> and
/// <see cref="NPCControl.vectordata"/> to 0 instead of 1.</item>
/// <item><see cref="EntityControl.CreateItem"/>: Changes the assignment of <see cref="NPCControl.data"/> so it sets the
/// entire array instead of the first element to not assume its presence anymore.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class MapEntitiesArraysLengthZeroTopLevelPatcher : ITopLevelPatcher
{
    private static readonly int[] InitialItemDataArray = [0];

    public void Patch()
    {
        // MonoDetour.HookGen has an issue where it can't disambiguate a ctor vs a cctor so we have to set up the hook manually.
        ConstructorInfo method = typeof(NPCControl).GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [],
            null)!;
        MonoDetour.HookGen.DefaultMonoDetourManager.Instance.ILHook(method, DefaultArrayToLengthZero);
        Md.EntityControl.CreateItem.ILHook(AssignEntireArrayOfData);
    }

    private static void DefaultArrayToLengthZero(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        // Patches data default length.
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.data)));
        cursor.GotoPrev(x => x.MatchLdcI4(out _));
        cursor.Set(OpCodes.Ldc_I4_0);

        // Patches vectordata default length.
        cursor.GotoNext(x => x.MatchStfld<NPCControl>(nameof(NPCControl.vectordata)));

        // Need to remove the first element initializer.
        cursor.Index--;
        while (cursor.Current.OpCode != OpCodes.Newarr)
        {
            cursor.Set(OpCodes.Nop);
            cursor.Index--;
        }

        cursor.GotoPrev(x => x.MatchLdcI4(1));
        cursor.Set(OpCodes.Ldc_I4_0);
    }

    private static void AssignEntireArrayOfData(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchLdfld<NPCControl>(nameof(NPCControl.data)));
        // The NPCControl should be on the stack by this point so we can just dup it.
        cursor.EmitDup();
        cursor.EmitDelegate(SetNpcControlDataArray);
    }

    private static void SetNpcControlDataArray(NPCControl npcControl) => npcControl.data = InitialItemDataArray;
}