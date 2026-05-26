using System.Collections.ObjectModel;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class AndGateOnFlagsMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDGate;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public ReadOnlyCollection<Branch<FlagLeaf>> FlagsInput { get; private set; } =
        new List<Branch<FlagLeaf>>().AsReadOnly();

    internal AndGateOnFlagsMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = -1;
        InternalStartingPosition = new(0f, 9999f, 0f);
        InternalActivationFlagId = -1;
        InternalData.AddRange([-2]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        List<Branch<FlagLeaf>> flagsInput = new();
        for (int i = 1; i < InternalData.Count; i++)
        {
            int value = InternalData[i];
            flagsInput.Add(new(flagsRegistry.LeavesByGameIds[Math.Abs(value)]));
        }

        ChangeFlagsInput(flagsInput);
    }

    public void ChangeFlagsInput(List<Branch<FlagLeaf>> flagsInput)
    {
        InternalData.RemoveRange(1, InternalData.Count - 1);

        foreach (Branch<FlagLeaf> negatableFlag in flagsInput)
            InternalData.Add(-negatableFlag.GameId);

        FlagsInput = flagsInput.AsReadOnly();
    }
}