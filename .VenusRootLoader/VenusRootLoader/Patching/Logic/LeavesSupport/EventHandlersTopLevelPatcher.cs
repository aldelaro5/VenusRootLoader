using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="EventLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="EventControl.StartEvent"/>: Changes the coroutine called at the end to reflect the delegates from the
/// <see cref="EventLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EventHandlersTopLevelPatcher : ITopLevelPatcher
{
    private static EventHandlersTopLevelPatcher _instance = null!;
    private readonly ILeavesRegistry<EventLeaf> _eventsRegistry;

    public EventHandlersTopLevelPatcher(ILeavesRegistry<EventLeaf> eventsRegistry)
    {
        _instance = this;
        _eventsRegistry = eventsRegistry;
    }

    public void Patch()
    {
        Md.EventControl.StartEvent.ILHook(PatchStartEvent);
    }

    private static void PatchStartEvent(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        MethodInfo startCoroutine = typeof(MonoBehaviour)
            .MethodByName(nameof(MonoBehaviour.StartCoroutine), [typeof(IEnumerator)])!;

        cursor.Goto(-1);
        cursor.GotoPrev(x => x.MatchCallOrCallvirt<MonoBehaviour>(nameof(MonoBehaviour.StartCoroutine)));
        cursor.Current.Operand = startCoroutine;
        int indexHook = cursor.Index;
        cursor.Index--;
        while (!cursor.Current.MatchLdarg(0))
        {
            if (cursor.Current.MatchLdarg(1))
            {
                cursor.Index--;
                continue;
            }

            cursor.Set(OpCodes.Nop);
            cursor.Index--;
        }

        cursor.EmitLdarg0();

        // The +1 is to account for the insertion we just did.
        cursor.Goto(indexHook + 1);
        cursor.EmitDelegate(StartEventHook);
    }

    private static IEnumerator StartEventHook(EventControl instance, int eventGameId) =>
        _instance._eventsRegistry.GetByGameId(eventGameId).Handler(instance);
}