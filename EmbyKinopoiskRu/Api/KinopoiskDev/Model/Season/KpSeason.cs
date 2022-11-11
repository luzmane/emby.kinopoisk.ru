using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season
{
    public class KpSeason
    {
        public long MovieId { get; set; }
        public int Number { get; set; }
        public int EpisodesCount { get; set; }
        public List<KpEpisode> Episodes { get; set; } = new();
    }
}
