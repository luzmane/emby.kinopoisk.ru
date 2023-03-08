using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person
{
    public class KpPerson
    {
        public string? Birthday { get; set; }
        public List<KpValued> BirthPlace { get; set; } = new();
        public string? Death { get; set; }
        public List<KpValued> DeathPlace { get; set; } = new();
        public List<KpFact> Facts { get; set; } = new();
        public long Id { get; set; }
        public List<KpMovie> Movies { get; set; } = new();
        public string? Name { get; set; }
        public string? EnName { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }


        public class KpMovie
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public string? AlternativeName { get; set; }
            public float? Rating { get; set; }
            public string? Description { get; set; }
            public string? EnProfession { get; set; }
        }
    }
}
