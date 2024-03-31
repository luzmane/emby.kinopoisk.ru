using System.Collections.Generic;
using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film
{
    [DebuggerDisplay("#{KinopoiskId}, {NameRu}")]
    internal sealed class KpFilm
    {
        /// <summary>
        /// Used as backdrop
        /// </summary>
        public string CoverUrl { get; set; }

        public List<KpCountry> Countries { get; set; } = new List<KpCountry>();
        public string Description { get; set; }
        public int? FilmLength { get; set; }
        public List<KpGenre> Genres { get; set; } = new List<KpGenre>();
        public string ImdbId { get; set; }
        public long KinopoiskId { get; set; }
        public string LogoUrl { get; set; }
        public string NameOriginal { get; set; }
        public string NameRu { get; set; }
        public string PosterUrl { get; set; }
        public string PosterUrlPreview { get; set; }
        public float? RatingKinopoisk { get; set; }
        public string RatingMpaa { get; set; }
        public string Slogan { get; set; }
        public string Type { get; set; }
        public int? Year { get; set; }
    }
}
