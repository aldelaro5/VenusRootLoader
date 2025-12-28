using VenusRootLoader.GameContent;

namespace VenusRootLoader.ContentBinding;

internal interface IContentBinder<T, U>
    where T : GameContent<U>
{
    T BindNew(string namedId, string creatorId);
    T BindExisting(U itemId);
}