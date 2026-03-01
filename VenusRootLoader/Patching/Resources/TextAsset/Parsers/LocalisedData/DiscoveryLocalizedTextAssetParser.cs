using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class DiscoveryLocalizedTextAssetParser : ILocalizedTextAssetParser<DiscoveryLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, DiscoveryLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[languageId].Name);
        sb.Append('@');
        for (int i = 0; i < leaf.LocalizedData[languageId].PaginatedDescription.Count; i++)
        {
            DiscoveryLeaf.DiscoveryDescriptionPage page = leaf.LocalizedData[languageId].PaginatedDescription[i];
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
        leaf.LocalizedData[languageId].Name = fields[0];

        leaf.LocalizedData[languageId].PaginatedDescription.Clear();
        int? lastPageRequiredFlag = null;
        int lastDelimiter = 0;
        string paginatedDescription = fields[1];
        while (true)
        {
            int nextDelimiter = paginatedDescription.IndexOfAny(['{', '}'], lastDelimiter);
            if (nextDelimiter == -1)
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = paginatedDescription[lastDelimiter..],
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.LocalizedData[languageId].PaginatedDescription.Add(descriptionPage);
                break;
            }

            if (paginatedDescription[nextDelimiter] == '{')
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = paginatedDescription.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.LocalizedData[languageId].PaginatedDescription.Add(descriptionPage);
                lastPageRequiredFlag = null;
                lastDelimiter = nextDelimiter + 1;
            }
            else
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = paginatedDescription.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.LocalizedData[languageId].PaginatedDescription.Add(descriptionPage);

                lastDelimiter = nextDelimiter + 1;
                int flagSlotDelimiter = paginatedDescription.IndexOf('}', lastDelimiter);
                lastPageRequiredFlag = int.Parse(
                    paginatedDescription.Substring(lastDelimiter, flagSlotDelimiter - lastDelimiter));
                lastDelimiter = flagSlotDelimiter + 1;
            }
        }
    }
}