namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film
{
    public class KpFilmStaff
    {
        /// <summary>
        /// KinopoiskId
        /// </summary>
        public long StaffId { get; set; }
        public string? NameRu { get; set; }
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? ProfessionKey { get; set; }
    }
}
