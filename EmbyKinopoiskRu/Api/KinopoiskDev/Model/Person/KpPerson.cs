using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person
{
    public class KpPerson
    {
        public List<KpValued>? BirthPlace { get; set; }
        public string? Birthday { get; set; }
        public string? Death { get; set; }
        public List<KpValued>? Facts { get; set; }
        public long Id { get; set; }
        public List<KpMovie>? Movies { get; set; }
        public string? Name { get; set; }
        public string? Photo { get; set; }


        public class KpMovie
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public string? AlternativeName { get; set; }
            public float Rating { get; set; }
            public string? Description { get; set; }
        }
    }
}
