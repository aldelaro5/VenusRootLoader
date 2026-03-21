using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

public sealed class AreaLeaf : Leaf
{
    public sealed class AreaLanguageData
    {
        public string Name { get; set; } = "";
        public List<string> PaginatedDescription { get; } = new();
    }

    internal AreaLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    public Vector2 MapPosition { get; set; }

    public LocalizedData<AreaLanguageData> LocalizedData { get; } = new();
}