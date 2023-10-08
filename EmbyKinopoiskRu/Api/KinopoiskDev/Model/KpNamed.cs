using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpNamed
    {
        public string Name { get; set; }

        private string DebuggerDisplay => $"{Name}";
    }
}
