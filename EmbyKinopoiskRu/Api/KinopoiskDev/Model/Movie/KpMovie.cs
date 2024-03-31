using System.Collections.Generic;
using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("#{Id}, {Name}")]
    internal sealed class KpMovie
    {
        public string AlternativeName { get; set; }
        public KpImage Backdrop { get; set; }
        public List<KpNamed> Countries { get; set; }
        public string Description { get; set; }
        public KpExternalId ExternalId { get; set; }
        public List<KpFact> Facts { get; set; }
        public List<KpNamed> Genres { get; set; }
        public long Id { get; set; }
        public List<string> Lists { get; set; }
        public KpImage Logo { get; set; }
        public int? MovieLength { get; set; }
        public string Name { get; set; }
        public List<KpPersonMovie> Persons { get; set; }
        public KpImage Poster { get; set; }
        public KpPremiere Premiere { get; set; }
        public List<KpCompany> ProductionCompanies { get; set; }
        public KpRating Rating { get; set; }
        public string RatingMpaa { get; set; }
        public List<KpYearRange> ReleaseYears { get; set; }
        public List<KpSequel> SequelsAndPrequels { get; set; }
        public string Slogan { get; set; }
        public int? Top250 { get; set; }
        public KpMovieType? TypeNumber { get; set; }
        public KpVideos Videos { get; set; }
        public int? Year { get; set; }
    }
}
