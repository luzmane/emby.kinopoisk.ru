using System.Collections.Generic;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season
{
    internal sealed class KpSeason
    {
        public string AirDate { get; set; }
        public string Description { get; set; }
        public List<KpEpisode> Episodes { get; set; }
        public int EpisodesCount { get; set; }
        public long MovieId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public KpImage Poster { get; set; }
    }
}
