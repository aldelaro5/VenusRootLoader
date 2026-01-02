using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Discoveries;

internal sealed class DiscoveryLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";

    internal List<DiscoveryDescriptionPage> DescriptionPages { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append('@');
        for (int i = 0; i < DescriptionPages.Count; i++)
        {
            DiscoveryDescriptionPage page = DescriptionPages[i];
            if (i == 0)
            {
                sb.Append(page.Text);
                continue;
            }

            if (page.RequiredFlagGameId.HasValue)
            {
                sb.Append('}');
                sb.Append(page.RequiredFlagGameId.Value);
                sb.Append('}');
            }
            else
            {
                sb.Append('{');
            }

            sb.Append(page.Text);
        }

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        Name = fields[0];

        DescriptionPages.Clear();
        int? lastPageRequiredFlag = null;
        int lastDelimiter = 0;
        while (true)
        {
            int nextDelimiter = text.IndexOfAny(['{', '}'], lastDelimiter);
            if (nextDelimiter == -1)
            {
                DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text[lastDelimiter..],
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                DescriptionPages.Add(descriptionPage);
                break;
            }

            if (text[nextDelimiter] == '{')
            {
                DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                DescriptionPages.Add(descriptionPage);
                lastPageRequiredFlag = null;
                lastDelimiter = nextDelimiter + 1;
            }
            else
            {
                DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                DescriptionPages.Add(descriptionPage);

                lastDelimiter = nextDelimiter + 1;
                int flagSlotDelimiter = text.IndexOf('}', lastDelimiter);
                lastPageRequiredFlag = int.Parse(text.Substring(lastDelimiter, flagSlotDelimiter - lastDelimiter));
                lastDelimiter = flagSlotDelimiter + 1;
            }
        }
    }
}