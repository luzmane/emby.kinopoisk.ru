using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using EmbyKinopoiskRu.YtDownloader.Model;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.YtDownloader
{
    internal class Y2MateDownloader : YoutubeDownloader
    {
        private const string AnalyzeUrlString = "https://www.y2mate.com/mates/en943/analyzeV2/ajax";
        private const string ConvertUrlString = "https://www.y2mate.com/mates/convertV2/index";
        private static readonly Regex Quality = new Regex(@"\d+p", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly int SizeStringLength = " MB".Length;

        internal Y2MateDownloader(ILogManager logManager, IHttpClient httpClient)
            : base(logManager, httpClient, "Y2MateDownloader")
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
                Host = "www.y2mate.com",
                Referer = "https://www.y2mate.com/en943",
                UserAgent = userAgent,
                TimeoutMs = 120_000,
                Url = AnalyzeUrlString,
                EnableKeepAlive = true,
                RequestContentType = "application/x-www-form-urlencoded",
                /*@formatter:off*/
                RequestHttpContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("k_query", $"https://www.youtube.com/watch?v={youtubeId}"),
                    new KeyValuePair<string, string>("k_page", "home"),
                    new KeyValuePair<string, string>("hl", "en"),
                    new KeyValuePair<string, string>("q_auto", "1")
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
            options.RequestHeaders.Add("Alt-Used", "www.y2mate.com");
            options.RequestHeaders.Add("Origin", "https://www.y2mate.com");
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
                    .Where(p => p.quality > 0 && p.tFormat?.Size?.Length > SizeStringLength)
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
                Host = "www.y2mate.com",
                Referer = $"https://www.y2mate.com/youtube/{youtubeId}",
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
            options.RequestHeaders.Add("Alt-Used", "www.y2mate.com");
            options.RequestHeaders.Add("Origin", "https://www.y2mate.com");
            options.RequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            options.RequestHeaders.Add("charset", "UTF-8");

            return options;
        }

        protected override string GetDownloadReferer()
        {
            return "https://www.y2mate.com/";
        }
    }
}
