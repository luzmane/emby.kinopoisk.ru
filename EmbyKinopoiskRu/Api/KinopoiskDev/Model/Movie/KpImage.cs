using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class KpImage
    {
        public string Url { get; set; }
        public string PreviewUrl { get; set; }

        private string DebuggerDisplay => $"Url is {string.IsNullOrWhiteSpace(Url)}, PreviewUrl is {string.IsNullOrWhiteSpace(PreviewUrl)}";
    }
}
