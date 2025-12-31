namespace VenusRootLoader.Utility;

internal static class StringUtils
{
    internal static readonly char[] CommaSplitDelimiter = [','];
    internal static readonly char[] SemiColonSplitDelimiter = [';'];
    internal static readonly char[] AtSymbolSplitDelimiter = ['@'];
    internal static readonly char[] NumberSignSplitDelimiter = ['#'];
    internal static readonly char[] OpeningBraceSplitDelimiter = ['{'];
    internal static readonly char[] ClosingBraceSplitDelimiter = ['}'];
    internal static readonly char[] NewlineSplitDelimiter = ['\n'];
}