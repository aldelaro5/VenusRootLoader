using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class AreaLeaf : Leaf
{
    public sealed class AreaLanguageData
    {
        public string Name { get; set; } = "";
        public List<string> PaginatedDescription { get; } = new();
    }

    internal AreaLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public Vector2 MapPosition { get; set; }

    public LocalizedData<AreaLanguageData> LocalizedData { get; } = new();
}