using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person
{
    public class KpPerson
    {
        public string? Birthday { get; set; }
        public string? BirthPlace { get; set; }
        public string? Death { get; set; }
        public string? DeathPlace { get; set; }
        public List<string>? Facts { get; init; }
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        /// <summary>
        /// KinopoiskId
        /// </summary>
        public long PersonId { get; set; }
        public string? PosterUrl { get; set; }

    }
}
