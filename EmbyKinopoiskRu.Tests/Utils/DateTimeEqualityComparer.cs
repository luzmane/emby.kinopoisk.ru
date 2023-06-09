using System.Diagnostics.CodeAnalysis;

namespace EmbyKinopoiskRu.Tests;

public class DateTimeEqualityComparer : IEqualityComparer<DateTime>
{
    public bool Equals(DateTime x, DateTime y)
    {
        return x.Year == y.Year
            && x.Month == y.Month
            && x.Day == y.Day;
    }

    public int GetHashCode([DisallowNull] DateTime obj)
    {
        return obj.GetHashCode();
    }
}
