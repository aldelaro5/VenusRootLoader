using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.Entities;

internal sealed class AnimIdResourcePreload : ITextAssetSerializable
{
    internal string ResourcePath { get; set; } = "";
    internal bool PreloadOnlyDuringBattles { get; set; }
    internal bool IsSprite { get; set; }

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