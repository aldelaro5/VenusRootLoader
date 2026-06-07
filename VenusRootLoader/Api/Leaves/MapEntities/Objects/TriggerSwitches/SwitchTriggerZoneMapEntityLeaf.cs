namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.TriggerSwitches;

public enum SwitchTriggerZoneMode
{
    ActivateOnLeave = 0,
    ActivateOnEnterDeactivateOnLeave = 1
}

public sealed class SwitchTriggerZoneMapEntityLeaf : TriggerSwitchMapEntityLeaf
{
    internal SwitchTriggerZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public SwitchTriggerZoneMode ActivationMode
    {
        get => (SwitchTriggerZoneMode)InternalData[1].Value;
        set => InternalData[1].Value = (int)value;
    }

    public bool DestroysBeemerangWhileInsideAndDeactivated
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(-1), new(1), new(0)]);
    }
}