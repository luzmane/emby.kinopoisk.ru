using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person
{
    public class KpPerson
    {
        public string Birthday { get; set; }
        public List<KpValued> BirthPlace { get; set; }
        public string Death { get; set; }
        public List<KpValued> DeathPlace { get; set; }
        public List<KpFact> Facts { get; set; }
        public long Id { get; set; }
        public List<KpMoviePerson> Movies { get; set; }
        public string Name { get; set; }
        public string EnName { get; set; }
        public string Photo { get; set; }
        public string Description { get; set; }

    }
}
