using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class ActionCommandLeaf : Leaf
{
    public delegate void ActionCommandSetupHandler(
        BattleControl battle,
        BattleControl.DoCommandSetupContext context);

    public delegate IEnumerator ActionCommandExecutionHandler(
        BattleControl battle,
        BattleControl.DoCommandExecutionContext context);

    internal ActionCommandLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public ActionCommandSetupHandler SetupHandler { get; set; } = null!;
    public ActionCommandExecutionHandler ExecutionHandler { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        ActionCommandSetupHandler setupHandler,
        ActionCommandExecutionHandler executionHandler)
    {
        SetupHandler = setupHandler;
        ExecutionHandler = executionHandler;
    }
}