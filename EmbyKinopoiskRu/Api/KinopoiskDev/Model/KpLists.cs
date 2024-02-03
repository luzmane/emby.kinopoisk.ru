using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpLists
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public int MoviesCount { get; set; }
        public KpImage Cover { get; set; }

        private string DebuggerDisplay => $"{Name} ({MoviesCount})";

    }
}
