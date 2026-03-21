using VenusRootLoader.Api.MapEntities;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

public interface IMapEntityTextAssetParser
{
    string GetTextAssetSerializedString(string subPath, MapEntity value);
    MapEntity FromTextAssetSerializedString(int id, string name, string text);
}