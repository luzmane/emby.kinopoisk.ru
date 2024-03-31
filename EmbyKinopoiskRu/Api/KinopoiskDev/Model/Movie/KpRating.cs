using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("Kp: {Kp}")]
    internal sealed class KpRating
    {
        /// <summary>
        /// Kinopoisk rating
        /// </summary>
        public float? Kp { get; set; }
    }
}
