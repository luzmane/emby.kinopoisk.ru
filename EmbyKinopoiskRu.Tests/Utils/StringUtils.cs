using System.Text.RegularExpressions;

namespace EmbyKinopoiskRu.Tests.Utils;

public static class StringUtils
{
    private static readonly Regex NotRusChars = new("[^А-Я]", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string NormalizeRusString(this string value)
    {
        return NotRusChars.Replace(value.ToUpperInvariant(), string.Empty);
    }
}
