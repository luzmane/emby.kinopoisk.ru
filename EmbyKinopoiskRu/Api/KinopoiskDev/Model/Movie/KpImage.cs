using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpImage
    {
        public string Url { get; set; }
        public string PreviewUrl { get; set; }

        private string DebuggerDisplay => $"Url is empty - {string.IsNullOrWhiteSpace(Url)}, PreviewUrl is empty - {string.IsNullOrWhiteSpace(PreviewUrl)}";
    }
}
