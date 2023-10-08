using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season
{
    internal sealed class KpSeason
    {
        public int Number { get; set; }
        public List<KpEpisode> Episodes { get; } = new List<KpEpisode>();
    }
}
