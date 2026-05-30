using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities;

public struct BoxColliderInfo
{
    public required bool IsTrigger { get; set; }
    public required Vector3 Size { get; set; }
    public required Vector3 Center { get; set; }
}