using System;
using System.Collections.Generic;

using EmbyKinopoiskRu.Api;

namespace EmbyKinopoiskRu.Helper
{
    internal class KpTrailerComparer : IEqualityComparer<KpTrailer>
    {
        public bool Equals(KpTrailer x, KpTrailer y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.GetType() != y.GetType())
            {
                return false;
            }

            return string.Equals(x.Url, y.Url, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(KpTrailer obj)
        {
            return (obj.Url != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Url) : 0);
        }
    }
}
