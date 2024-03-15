using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

using System.Diagnostics;

namespace EmbyKinopoiskRu.Api
{
    [DebuggerDisplay("{Name} ({MoviesCount})")]
    internal sealed class KpLists
    {
        public string Category { get; set; }
        public KpImage Cover { get; set; }
        public int MoviesCount { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }

    }
}
