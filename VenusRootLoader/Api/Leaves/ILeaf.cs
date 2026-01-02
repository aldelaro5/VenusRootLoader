namespace VenusRootLoader.Api.Leaves;

internal interface ILeaf<TGameId>
{
    TGameId GameId { get; init; }
    string NamedId { get; init; }
    string CreatorId { get; init; }
}