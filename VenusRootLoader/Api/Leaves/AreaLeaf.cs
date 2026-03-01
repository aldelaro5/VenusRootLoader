using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

public sealed class AreaLeaf : Leaf
{
    public sealed class AreaLanguageData
    {
        public string Name { get; set; } = "";
        public List<string> PaginatedDescription { get; } = new();
    }

    public Vector2 MapPosition { get; set; }

    public LocalizedData<AreaLanguageData> LocalizedData { get; } = new();
}