using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season
{
    internal sealed class KpEpisode
    {
        public string AirDate { get; set; }
        public string EnName { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public KpImage Still { get; set; }

    }
}
