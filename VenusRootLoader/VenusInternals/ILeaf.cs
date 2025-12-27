namespace VenusRootLoader.VenusInternals;

internal interface ILeaf<T>
{
    string CreatorId { get; }
    string OwnerId { get; }
    string NamedId { get; }
    T GameId { get; }
}