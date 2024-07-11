using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using EmbyKinopoiskRu.TrailerDownloader.Youtube.Model;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.TrailerDownloader.Youtube
{
    internal class Tomp3Downloader : YoutubeDownloader
    {
        private const string AnalyzeUrlString = "https://tomp3.cc/api/ajax/search";
        private const string ConvertUrlString = "https://tomp3.cc/api/ajax/convert";
        private static readonly Regex Quality = new Regex(@"\d+p", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Tomp3Downloader(ILogManager logManager, IHttpClient httpClient)
            : base(logManager, httpClient, "Tomp3Downloader")
        {
        }

        protected override HttpRequestOptions GetAnalyzeRequestOptions(string youtubeId, string userAgent, CancellationToken cancellationToken)
        {
            var options = new HttpRequestOptions
            {
                AcceptHeader = "*/*",
                CacheMode = CacheMode.None,
                CancellationToken = cancellationToken,
                DecompressionMethod = CompressionMethod.Gzip,
                EnableHttpCompression = true,
                Host = "tomp3.cc",
                Referer = $"https://tomp3.cc/youtube-downloader/{youtubeId}",
                UserAgent = userAgent,
                TimeoutMs = 120_000,
                Url = AnalyzeUrlString,
                EnableKeepAlive = true,
                RequestContentType = "application/x-www-form-urlencoded",
                /*@formatter:off*/
                RequestHttpContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("query", $"https://www.youtube.com/watch?v={youtubeId}"),
                    new KeyValuePair<string, string>("vt", "downloader")
                }),
                /*@formatter:on*/
                EnableDefaultUserAgent = false,
                LogRequestAsDebug = true,
                Sanitation =
                {
                    SanitizeDefaultParams = false
                }
            };
            options.RequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            options.RequestHeaders.Add("Sec-Fetch-Mode", "cors");
            options.RequestHeaders.Add("Sec-Fetch-Dest", "");
            options.RequestHeaders.Add("TE", "trailers");
            options.RequestHeaders.Add("Origin", "https://tomp3.cc");
            options.RequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            options.RequestHeaders.Add("charset", "UTF-8");

            return options;
        }

        protected override Dictionary<int, TrailerFormat> ParseLinksAnalyzeResponse(JsonElement linksElement)
        {
            return linksElement.TryGetProperty("mp4", out var mp4)
                ? mp4.EnumerateObject()
                    .Select(p => p.Value.Deserialize<TrailerFormat>())
                    .Where(p => Quality.IsMatch(p.Quality))
                    .Select(tFormat => int.TryParse(tFormat.Quality.Replace("p", ""), out int quality) ? (quality, tFormat) : (quality: 0, tFormat: null))
                    .Where(p => p.quality > 0)
                    .GroupBy(p => p.quality)
                    .Select(grp => grp.FirstOrDefault())
                    .ToDictionary(p => p.quality, p => p.tFormat)
                : new Dictionary<int, TrailerFormat>();
        }

        protected override HttpRequestOptions GetConvertRequestOptions(string youtubeId, string videoKey, string userAgent, CancellationToken cancellationToken)
        {
            var options = new HttpRequestOptions
            {
                AcceptHeader = "*/*",
                CacheMode = CacheMode.None,
                CancellationToken = cancellationToken,
                DecompressionMethod = CompressionMethod.Gzip,
                EnableHttpCompression = true,
                Host = "tomp3.cc",
                Referer = $"https://tomp3.cc/youtube-downloader/{youtubeId}",
                UserAgent = userAgent,
                TimeoutMs = 120_000,
                Url = ConvertUrlString,
                EnableKeepAlive = true,
                RequestContentType = "application/x-www-form-urlencoded",
                /*@formatter:off*/
                RequestHttpContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("vid", youtubeId),
                    new KeyValuePair<string, string>("k", videoKey)
                }),
                /*@formatter:on*/
                EnableDefaultUserAgent = false,
                LogRequestAsDebug = true,
                Sanitation =
                {
                    SanitizeDefaultParams = false
                }
            };
            options.RequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            options.RequestHeaders.Add("Sec-Fetch-Mode", "cors");
            options.RequestHeaders.Add("Sec-Fetch-Dest", "");
            options.RequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            options.RequestHeaders.Add("charset", "UTF-8");
            options.RequestHeaders.Add("TE", "trailers");
            options.RequestHeaders.Add("Origin", "https://tomp3.cc");

            return options;
        }

        protected override string GetDownloadReferer()
        {
            return "https://tomp3.cc/";
        }
    }
}
