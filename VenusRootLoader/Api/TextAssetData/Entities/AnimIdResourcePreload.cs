using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Entities;

public sealed class AnimIdResourcePreload : ITextAssetSerializable
{
    public string ResourcePath { get; set; } = "";
    public bool PreloadOnlyDuringBattles { get; set; }
    public bool IsSprite { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        if (PreloadOnlyDuringBattles)
            sb.Append('$');
        if (IsSprite)
            sb.Append('&');
        sb.Append(ResourcePath);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        StringBuilder sb = new(text);
        if (sb.Length > 0 && sb[0] == '$')
        {
            PreloadOnlyDuringBattles = true;
            sb.Remove(0, 1);
        }

        if (sb.Length > 0 && sb[0] == '&')
        {
            IsSprite = true;
            sb.Remove(0, 1);
        }

        ResourcePath = sb.ToString();
    }
}