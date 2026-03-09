using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class QuestLocalizedTextAssetParser : ILocalizedTextAssetParser<QuestLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, QuestLeaf leaf)
    {
        QuestLeaf.QuestLanguageData questLanguageData = leaf.LocalizedData[languageId];
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

        sb.Append('@');
        sb.Append(questLanguageData.Sender);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, QuestLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId] = new()
        {
            Name = fields[0],
            Sender = fields[2]
        };
        leaf.LocalizedData[languageId].PaginatedDescription.Clear();

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
                    RequiredFlagGameId = lastPageRequiredFlag
                };
                leaf.LocalizedData[languageId].PaginatedDescription.Add(descriptionPage);
                break;
            }

            if (paginatedDescription[nextDelimiter] == '{')
            {
                QuestLeaf.QuestDescriptionPage descriptionPage = new()
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
                QuestLeaf.QuestDescriptionPage descriptionPage = new()
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