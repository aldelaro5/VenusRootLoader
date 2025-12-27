using VenusRootLoader.GameContent;

namespace VenusRootLoader.ContentBinding;

internal interface IContentBinder<T, U>
    where T : IGameContent<U>
{
    T BindNew(string namedId);
    T BindExisting(U itemId);
}