using System.Collections.Generic;
using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class KpMovie
    {
        public string? AlternativeName { get; set; }
        public KpImage? Backdrop { get; set; }
        public List<KpNamed> Countries { get; set; } = new();
        public string? Description { get; set; }
        public KpExternalId? ExternalId { get; set; }
        
        /// <summary>
        /// todo: use facts in film description
        /// </summary>
        public List<KpFact> Facts { get; set; } = new();
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
        public List<Sequel> SequelsAndPrequels { get; set; } = new();
        public string? Slogan { get; set; }
        public int? Top250 { get; set; }
        public KpMovieType? TypeNumber { get; set; }
        public KpVideos? Videos { get; set; }
        public int Year { get; set; }


        private string DebuggerDisplay => $"#{Id}, {Name}";


        public class KpPerson
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public string? EnName { get; set; }
            public string? Photo { get; set; }
            public string? Profession { get; set; }
            public string? EnProfession { get; set; }
            public string? Description { get; set; }
        }

    }
}
