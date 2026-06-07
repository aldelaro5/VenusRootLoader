namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

// TODO: Remember to point out in the xmldocs that the regional gets set upon actuation
public sealed class TimerSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal TimerSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public int TimerInFramesBeforeAutomaticTurnOff
    {
        get => InternalData[2].Value;
        set => InternalData[2].Value = value;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(0), new(30), new(0), new(0)]);
    }
}