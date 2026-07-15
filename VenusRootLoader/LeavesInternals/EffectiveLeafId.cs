using CommunityToolkit.Diagnostics;
using VenusRootLoader.Utility;

namespace VenusRootLoader.LeavesInternals;

internal static class EffectiveLeafId
{
    private static readonly char[] DisallowedCharactersInEffectiveIds =
        ['<', '>', ':', '"', '/', '\\', '|', '?', '*', ','];

    private static readonly string DisallowedCharactersListString =
        string.Join(", ", DisallowedCharactersInEffectiveIds);

    internal static void EnsureIdPartIsValid(string idPart, string idPartName)
    {
        Guard.IsNotNullOrWhiteSpace(idPart);

        // Windows path restrictions
        int indexDisallowedCharacter = idPart.IndexOfAny(DisallowedCharactersInEffectiveIds);
        if (indexDisallowedCharacter != -1)
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} contains any of the following " +
                $"characters which is not allowed: {DisallowedCharactersListString}");
        }

        if (idPart.EndsWith("."))
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} ends with a \".\" which is not allowed");
        }

        if (idPart.Any(char.IsControl))
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} contains control characters which is not allowed");
        }

        // Removes whitespace ambiguities and prevents leading or trailing whitespaces which is problematic
        if (idPart.Any(char.IsWhiteSpace))
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} contains whitespaces which is not allowed");
        }

        // Reserved effective id separator restriction
        if (idPart.Contains(Constants.LeafEffectiveIdSeparator))
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} contains {Constants.LeafEffectiveIdSeparator} which is not " +
                $"allowed because it is a reserved character for internal usage");
        }

        // Enum name compatibility
        char firstCharacter = idPart[0];
        if (char.IsDigit(firstCharacter) || firstCharacter == '-' || firstCharacter == '+')
        {
            ThrowHelper.ThrowFormatException(
                $"The {idPartName} with a value of {idPart} starts with a digit, \"-\" or \"+\" which is not allowed");
        }
    }

    internal static string CreateFromParts(string creatorId, string namedId) =>
        $"{creatorId}{Constants.LeafEffectiveIdSeparator}{namedId}";

    internal static string CreateBaseGameEffectiveId(string namedId) =>
        $"{Constants.BaseGameId}{Constants.LeafEffectiveIdSeparator}{namedId}";

    internal static (string CreatorId, string NamedId) SplitParts(string effectiveId)
    {
        string[] parts = effectiveId.Split(StringUtils.LeafEffectiveIdDelimiter, StringSplitOptions.RemoveEmptyEntries);
        return (parts[0], parts[1]);
    }
}