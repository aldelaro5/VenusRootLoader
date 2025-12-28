namespace VenusRootLoader.GameContent;

internal abstract class GameContent<T>
{
    internal required T GameId { get; init; }
    internal required string NamedId { get; init; }
    internal required string CreatorId { get; init; }
}