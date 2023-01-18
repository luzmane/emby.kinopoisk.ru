using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    public class KpMovie
    {
        public string? AlternativeName { get; set; }
        public KpImage? Backdrop { get; set; }
        public List<KpNamed> Countries { get; set; } = new();
        public string? Description { get; set; }
        public string? EnName { get; set; }
        public KpExternalId? ExternalId { get; set; }
        public List<KpNamed> Genres { get; set; } = new();
        public long Id { get; set; }
        public KpImage? Logo { get; set; }
        public int MovieLength { get; set; }
        public string? Name { get; set; }
        public List<KpPerson> Persons { get; set; } = new();
        public KpImage? Poster { get; set; }
        public KpPremiere? Premiere { get; set; }
        public List<KpCompany> ProductionCompanies { get; set; } = new();
        public KpRating? Rating { get; set; }
        public string? RatingMpaa { get; set; }
        public List<KpYearRange> ReleaseYears { get; set; } = new();
        public string? Slogan { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public KpMovieType? TypeNumber { get; set; }
        public KpVideos? Videos { get; set; }
        public int Year { get; set; }


        public class KpPerson
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public string? EnName { get; set; }
            public string? Photo { get; set; }
            public string? EnProfession { get; set; }
            public string? Description { get; set; }
        }

    }
}
