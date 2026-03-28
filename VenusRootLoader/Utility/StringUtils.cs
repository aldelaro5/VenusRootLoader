namespace VenusRootLoader.Utility;

/// <summary>
/// Contains static instances of common char arrays
/// used with the <see cref="string.Split(char[], int)"/> method to avoid unecessary allocations.
/// </summary>
internal static class StringUtils
{
    internal static readonly char[] CommaSplitDelimiter = [','];
    internal static readonly char[] SemiColonSplitDelimiter = [';'];
    internal static readonly char[] AtSymbolSplitDelimiter = ['@'];
    internal static readonly char[] NumberSignSplitDelimiter = ['#'];
    internal static readonly char[] OpeningBraceSplitDelimiter = ['{'];
    internal static readonly char[] ClosingBraceSplitDelimiter = ['}'];
    internal static readonly char[] NewlineSplitDelimiter = ['\n'];
    internal static readonly char[] QuestionMarkSplitDelimiter = ['?'];
}