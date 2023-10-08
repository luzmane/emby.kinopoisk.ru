using System.Collections.Generic;

namespace EmbyKinopoiskRu.Helper
{
    internal sealed class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, long>>
    {
        public bool Equals(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
        {
            return string.Equals(x.Key, y.Key);
        }

        public int GetHashCode(KeyValuePair<string, long> obj)
        {
            return obj.Key == null
            ? 0
            : obj.Key.GetHashCode();
        }
    }
}
