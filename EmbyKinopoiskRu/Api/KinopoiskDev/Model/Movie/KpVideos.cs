using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    public class KpVideos
    {
        public List<KpVideo> Trailers { get; init; } = new();
        public List<KpVideo> Teasers { get; init; } = new();
    }

}
