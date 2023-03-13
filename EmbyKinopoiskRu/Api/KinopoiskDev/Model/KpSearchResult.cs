using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    public class KpSearchResult<TItem>
    {
        public List<TItem> Docs { get; init; } = new();
        public int Limit { get; set; }
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }
}
