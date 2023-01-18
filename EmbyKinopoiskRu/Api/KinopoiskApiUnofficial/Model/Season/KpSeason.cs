using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season
{
    public class KpSeason
    {
        public int Number { get; set; }
        public List<KpEpisode> Episodes { get; set; } = new();
    }
}
