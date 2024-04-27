using System.Text.Json.Serialization;

namespace EmbyKinopoiskRu.YtDownloader.Model
{
    internal class TrailerFormat
    {
        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("f")]
        public string Format { get; set; }

        [JsonPropertyName("q")]
        public string Quality { get; set; }

        [JsonPropertyName("k")]
        public string Key { get; set; }
    }
}
