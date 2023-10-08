namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person
{
    internal sealed class KpMoviePerson
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AlternativeName { get; set; }
        public float? Rating { get; set; }
        public string Description { get; set; }
        public string EnProfession { get; set; }
    }
}
