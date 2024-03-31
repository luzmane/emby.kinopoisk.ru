using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{Name}")]
    internal sealed class KpCompany
    {
        public string Name { get; set; }
    }
}
