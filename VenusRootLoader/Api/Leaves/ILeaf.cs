namespace VenusRootLoader.Api.Leaves;

internal interface ILeaf
{
    int GameId { get; init; }
    string NamedId { get; init; }
    string CreatorId { get; init; }
}