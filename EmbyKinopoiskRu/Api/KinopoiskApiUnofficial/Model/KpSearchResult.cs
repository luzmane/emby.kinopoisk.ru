using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model
{
    public class KpSearchResult<TItem>
    {
        public List<TItem> Items { get; set; } = new();
    }
}
