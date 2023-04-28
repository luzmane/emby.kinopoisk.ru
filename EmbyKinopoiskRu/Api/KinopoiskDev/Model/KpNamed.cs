using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class KpNamed
    {
        public string Name { get; set; }

        private string DebuggerDisplay => $"{Name}";
    }
}
