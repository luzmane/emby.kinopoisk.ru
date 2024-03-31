using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model
{
    [DebuggerDisplay("{Name}")]
    internal sealed class KpNamed
    {
        public string Name { get; set; }
    }
}
