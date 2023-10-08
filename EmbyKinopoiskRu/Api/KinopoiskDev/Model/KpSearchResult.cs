using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    internal sealed class KpSearchResult<TItem>
    {
        public List<TItem> Docs { get; set; } = new List<TItem>();
        public int Limit { get; set; }
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
        public bool HasError { get; set; }
    }
}
