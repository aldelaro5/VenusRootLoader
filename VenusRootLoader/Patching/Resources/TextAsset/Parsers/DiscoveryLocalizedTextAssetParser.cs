using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class DiscoveryLocalizedTextAssetParser : ILocalizedTextAssetParser<DiscoveryLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, DiscoveryLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.Name[languageId]);
        sb.Append('@');
        for (int i = 0; i < leaf.PaginatedDescription[languageId].Count; i++)
        {
            DiscoveryLeaf.DiscoveryDescriptionPage page = leaf.PaginatedDescription[languageId][i];
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

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, DiscoveryLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.Name[languageId] = fields[0];

        leaf.PaginatedDescription[languageId].Clear();
        int? lastPageRequiredFlag = null;
        int lastDelimiter = 0;
        while (true)
        {
            int nextDelimiter = text.IndexOfAny(['{', '}'], lastDelimiter);
            if (nextDelimiter == -1)
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text[lastDelimiter..],
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.PaginatedDescription[languageId].Add(descriptionPage);
                break;
            }

            if (text[nextDelimiter] == '{')
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.PaginatedDescription[languageId].Add(descriptionPage);
                lastPageRequiredFlag = null;
                lastDelimiter = nextDelimiter + 1;
            }
            else
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = text.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.PaginatedDescription[languageId].Add(descriptionPage);

                lastDelimiter = nextDelimiter + 1;
                int flagSlotDelimiter = text.IndexOf('}', lastDelimiter);
                lastPageRequiredFlag = int.Parse(text.Substring(lastDelimiter, flagSlotDelimiter - lastDelimiter));
                lastDelimiter = flagSlotDelimiter + 1;
            }
        }
    }
}