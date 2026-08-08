using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class DiscoveryLocalizedTextAssetParser : ILocalizedTextAssetParser<DiscoveryLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public DiscoveryLocalizedTextAssetParser(
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _languageRegistry = languageRegistry;
        _flagsRegistry = flagsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, DiscoveryLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name);
        sb.Append('@');
        for (int i = 0;
             i < leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Count;
             i++)
        {
            DiscoveryLeaf.DiscoveryDescriptionPage page = leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)]
                .PaginatedDescription[i];
            if (i == 0)
            {
                sb.Append(page.Text);
                continue;
            }

            if (page.RequiredFlag is not null)
            {
                sb.Append('}');
                sb.Append(page.RequiredFlag.Resolve().GameId);
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
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name = fields[0];

        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Clear();
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
                    RequiredFlag = lastPageRequiredFlag is null
                        ? (Branch<FlagLeaf>?)null
                        : _flagsRegistry.GetByGameId(lastPageRequiredFlag.Value)
                };
                leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Add(descriptionPage);
                break;
            }

            if (paginatedDescription[nextDelimiter] == '{')
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = paginatedDescription.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlag = lastPageRequiredFlag is null
                        ? (Branch<FlagLeaf>?)null
                        : _flagsRegistry.GetByGameId(lastPageRequiredFlag.Value)
                };
                leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Add(descriptionPage);
                lastPageRequiredFlag = null;
                lastDelimiter = nextDelimiter + 1;
            }
            else
            {
                DiscoveryLeaf.DiscoveryDescriptionPage descriptionPage = new()
                {
                    Text = paginatedDescription.Substring(lastDelimiter, nextDelimiter - lastDelimiter),
                    RequiredFlag = lastPageRequiredFlag is null
                        ? (Branch<FlagLeaf>?)null
                        : _flagsRegistry.GetByGameId(lastPageRequiredFlag.Value)
                };
                leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Add(descriptionPage);

                lastDelimiter = nextDelimiter + 1;
                int flagSlotDelimiter = paginatedDescription.IndexOf('}', lastDelimiter);
                lastPageRequiredFlag = int.Parse(
                    paginatedDescription.Substring(lastDelimiter, flagSlotDelimiter - lastDelimiter));
                lastDelimiter = flagSlotDelimiter + 1;
            }
        }
    }
}