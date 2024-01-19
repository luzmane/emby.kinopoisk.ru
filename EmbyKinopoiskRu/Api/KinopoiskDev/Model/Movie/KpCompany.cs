using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpCompany
    {
        public string Name { get; set; }

        private string DebuggerDisplay => $"{Name}";
    }
}
