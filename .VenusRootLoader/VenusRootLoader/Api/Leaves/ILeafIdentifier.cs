namespace VenusRootLoader.Api.Leaves;

public interface ILeafIdentifier
{
    int GameId { get; }
    string NamedId { get; }
    string CreatorId { get; }
}