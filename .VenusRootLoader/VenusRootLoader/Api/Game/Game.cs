using VenusRootLoader.Api.Leaves.RankBonuses;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Game;

public static class Game
{
    public static readonly ILeafList<RankBonusLeaf> RankBonuses = RegistryResolver.ResolveList<RankBonusLeaf>();
}