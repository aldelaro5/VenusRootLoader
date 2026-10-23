using MonoDetour.Cil;
using MonoMod.Cil;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="ListTypeLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.ShowItemList"/>: Changes the setup and render switches to reflect the delegates from the
/// <see cref="ListTypeLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class ListTypesTopLevelPatcher : ITopLevelPatcher
{
    private static ListTypesTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<ListTypeLeaf> _listTypesRegistry;

    public ListTypesTopLevelPatcher(ILeavesRegistry<ListTypeLeaf> listTypesRegistry)
    {
        _instance = this;
        _listTypesRegistry = listTypesRegistry;
    }

    public void Patch()
    {
        Md.MainManager.ShowItemList.ILHook(PatchShowItemList);
    }

    private static void PatchShowItemList(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchSetupEndLabel = (ILLabel)cursor.Current.Operand;
        cursor.GotoPrev(x => x.MatchStfld(out _));
        int contextSetupLocalIndex = 0;
        cursor.GotoNext(x => x.MatchStloc(out contextSetupLocalIndex));
        cursor.Index++;
        cursor.EmitLdloc(contextSetupLocalIndex);
        cursor.Index++;
        cursor.EmitDelegate(ShowItemListSetupHandlerHook);
        cursor.EmitBr(switchSetupEndLabel);
        cursor.EmitLdarg0();

        cursor.Index++;
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchRenderEndLabel = (ILLabel)cursor.Current.Operand;
        cursor.GotoPrev(x => x.MatchStfld(out _));
        int contextRenderLocalIndex = 0;
        cursor.GotoNext(x => x.MatchStloc(out contextRenderLocalIndex));
        cursor.Index++;
        cursor.EmitLdloc(contextRenderLocalIndex);
        cursor.Index++;
        cursor.EmitDelegate(ShowItemListRendererHook);
        cursor.EmitBr(switchRenderEndLabel);
        cursor.EmitLdarg0();
    }

    private static void ShowItemListSetupHandlerHook(MainManager.ShowItemListSetupContext context, int listTypeGameId)
    {
        _instance._listTypesRegistry.GetByGameId(listTypeGameId).SetupHandler.Invoke(context);
    }

    private static void ShowItemListRendererHook(MainManager.ShowItemListRenderContext context, int listTypeGameId)
    {
        _instance._listTypesRegistry.GetByGameId(listTypeGameId).Renderer?.Invoke(context);
    }
}