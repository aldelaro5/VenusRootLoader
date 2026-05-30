using VenusRootLoader.Api.MapEntities;

namespace VenusRootLoader.Api.Common;

public sealed class NegatableMapEntityActivation
{
    public required MapEntity MapEntity { get; set; }
    public required bool IsActivationValueNegated { get; set; }
    internal int EffectiveValue => MapEntity.GameId * (IsActivationValueNegated ? -1 : 1);
}