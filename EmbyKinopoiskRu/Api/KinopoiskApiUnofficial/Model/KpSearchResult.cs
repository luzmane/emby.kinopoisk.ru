using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model
{
    internal sealed class KpSearchResult<TItem>
    {
        public List<TItem> Items { get; set; } = new List<TItem>();
    }
}
