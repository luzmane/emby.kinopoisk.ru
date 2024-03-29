using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpRating
    {
        /// <summary>
        /// Kinopoisk rating
        /// </summary>
        public float? Kp { get; set; }


        private string DebuggerDisplay => $"Kp: {Kp}";
    }
}
