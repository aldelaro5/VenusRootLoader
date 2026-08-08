using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class QuestLocalizedTextAssetParser : ILocalizedTextAssetParser<QuestLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public QuestLocalizedTextAssetParser(
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _languageRegistry = languageRegistry;
        _flagsRegistry = flagsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, QuestLeaf leaf)
    {
        QuestLeaf.QuestLanguageData questLanguageData = leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)];
        StringBuilder sb = new();
        sb.Append(questLanguageData.Name);
        sb.Append('@');
        for (int i = 0; i < questLanguageData.PaginatedDescription.Count; i++)
        {
            QuestLeaf.QuestDescriptionPage page = questLanguageData.PaginatedDescription[i];
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

        sb.Append('@');
        sb.Append(questLanguageData.Sender);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, QuestLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new()
        {
            Name = fields[0],
            Sender = fields[2]
        };
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Clear();

        int? lastPageRequiredFlag = null;
        int lastDelimiter = 0;
        string paginatedDescription = fields[1];
        while (true)
        {
            int nextDelimiter = paginatedDescription.IndexOfAny(['{', '}'], lastDelimiter);
            if (nextDelimiter == -1)
            {
                QuestLeaf.QuestDescriptionPage descriptionPage = new()
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
                QuestLeaf.QuestDescriptionPage descriptionPage = new()
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
                QuestLeaf.QuestDescriptionPage descriptionPage = new()
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