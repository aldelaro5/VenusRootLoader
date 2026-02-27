using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

public sealed class AreaLeaf : Leaf
{
    public class AreaLanguageData
    {
        public string Name { get; set; } = "";
        public List<string> PaginatedDescription { get; } = new();
    }

    internal Vector3 MapPosition { get; set; }

    public Dictionary<int, AreaLanguageData> LanguageData { get; } = new();
}