using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Switches;

// TODO: Remember to point out in the xmldocs that the regional gets set upon activation
public sealed class TimerSwitchMapEntityLeaf : SwitchMapEntityLeaf
{
    internal TimerSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public int TimeInFramesBeforeAutomaticDeactivation
    {
        get => InternalData[2].Value;
        set => InternalData[2].Value = value;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        int timeInFramesBeforeAutomaticDeactivation)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange([new(0), new(0), new(30), new(0), new(0)]);
        TimeInFramesBeforeAutomaticDeactivation = timeInFramesBeforeAutomaticDeactivation;
    }
}