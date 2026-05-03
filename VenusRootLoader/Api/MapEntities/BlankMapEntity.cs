using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

// TODO: Remove when every map entity type has their derived class
public sealed class BlankMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => OriginalType;
    internal override NPCControl.ObjectTypes ObjectType => OriginalObjectType;

    internal BlankMapEntity() { }
    internal override void InitializeFromNew() { }
    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}