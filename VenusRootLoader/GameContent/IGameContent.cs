namespace VenusRootLoader.GameContent;

internal interface IGameContent<T>
{
    T GameId { get; }
}