namespace VenusRootLoader.Leaves;

public interface ILeaf<T>
{
    string CreatorId { get; }
    string OwnerId { get; }
    string NamedId { get; }
    T GameId { get; }
}