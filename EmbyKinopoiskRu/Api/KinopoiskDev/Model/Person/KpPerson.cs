using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person
{
    public class KpPerson
    {
        public string? Birthday { get; set; }
        public List<KpValued>? BirthPlace { get; init; }
        public string? Death { get; set; }
        public List<KpValued>? DeathPlace { get; init; }
        public List<KpFact>? Facts { get; init; }
        public long Id { get; set; }
        public List<KpMoviePerson>? Movies { get; init; }
        public string? Name { get; set; }
        public string? EnName { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }

    }
}
