using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api;

public partial class Venus
{
    [MapEntityRegisterMethod]
    private TMapEntity RegisterMapEntity<TMapEntity>(string namedId, MapLeaf map)
        where TMapEntity : MapEntityLeaf
    {
        TMapEntity mapEntityLeaf = map.EntitiesRegistry.RegisterNew<TMapEntity>(namedId, BudId);
        mapEntityLeaf.BaseGameObjectName = namedId;
        mapEntityLeaf.Map = map;
        return mapEntityLeaf;
    }
}