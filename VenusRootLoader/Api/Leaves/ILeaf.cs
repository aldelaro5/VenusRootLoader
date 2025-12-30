namespace VenusRootLoader.Api.Leaves;

internal interface ILeaf<TGameId>
{
    TGameId GameId { get; }
    string NamedId { get; }
    string CreatorId { get; }
}