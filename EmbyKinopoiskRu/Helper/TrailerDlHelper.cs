using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Helper
{
    internal static class TrailerDlHelper
    {
        private static readonly Regex YoutubeWatch = new Regex(@"https?://www\.youtube\.com/watch\?[^&]*&?v=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex YoutubeEmbed = new Regex(@"https?://www\.youtube\.com/embed/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex YoutubeV = new Regex(@"https?://www\.youtube\.com/v/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex YoutuBe = new Regex(@"https?://youtu\.be/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex KpWidgetPlayer = new Regex(@"https?://widgets\.kinopoisk\.ru/discovery/trailer/(?<id>\d+)\?.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex S3YandexNet = new Regex(@"https?://trailers\.s3\.mds\.yandex\.net/video_original/(?<id>\d+)-.+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Random Random = new Random();
        private static readonly Regex InvalidFileNameChars = new Regex($"[{string.Join(string.Empty, Path.GetInvalidFileNameChars())}]+", RegexOptions.Compiled);

        internal const string NotExists = ".not_exists";

        internal static readonly List<string> TrailerStopWordList = new List<string>
        {
            "фрагмент",
            "интервью",
            "видео со съемок",
            "fragment",
            "interview",
        };

        internal static readonly List<string> RussianStopWordList = new List<string>
        {
            "испанский",
            "итальянский",
            "португальский",
            "японский",
            "тайский",
            "немецкий",
            "французский",
            "британский",
        };

        internal static bool CheckTrailerDuration(long durationSec, long maxTrailerDuration)
        {
            return Constants.OneMinuteInSec <= durationSec
                && durationSec <= maxTrailerDuration;
        }

        internal static string GetYoutubeId(string youtubeUrl)
        {
            return YoutubeWatch.Replace(
                YoutubeV.Replace(
                    YoutubeEmbed.Replace(
                        YoutuBe.Replace(
                            youtubeUrl,
                            string.Empty),
                        string.Empty),
                    string.Empty),
                string.Empty);
        }

        internal static string GetIntroName(string videoName, string videoId, string extension)
        {
            return $"{InvalidFileNameChars.Replace(videoName, string.Empty)} [{videoId}].{extension}";
        }

        #region User Agent

        internal static readonly List<string> UserAgents = new List<string>
        {
            "Mozilla/5.0 (X11; Linux x86_64; rv:124.0) Gecko/20100101 Firefox/127.0", // firefox
            "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.2592.81", // edge
            "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36", // chrome
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 OPR/111.0.0.0", //opera
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.8; rv:124.0) Gecko/20100101 Firefox/127.0" // mac
        };

        private static async Task<string> GetUserAgentFromApi(IHttpClient httpClient, ILogger logger, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(Plugin.Instance.Configuration.UserAgentApiKey))
            {
                logger.Warn("The key for UserAgent API is empty. Skip request to API");
                return string.Empty;
            }

            try
            {
                var options = new HttpRequestOptions
                {
                    AcceptHeader = "*/*",
                    CancellationToken = cancellationToken,
                    EnableHttpCompression = false,
                    Url = "https://api.apilayer.com/user_agent/generate?desktop=true",
                    CacheMode = CacheMode.None,
                    TimeoutMs = 5_000,
                    LogRequestAsDebug = true
                };
                options.RequestHeaders.Add("apikey", Plugin.Instance.Configuration.UserAgentApiKey);
                using (HttpResponseInfo response = await httpClient.GetResponse(options))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken))
                        {
                            var rootElement = jsonDocument.RootElement;
                            if (rootElement.TryGetProperty("ua", out var ua) && !ua.ValueEquals(string.Empty))
                            {
                                logger.Info("Successfully fetched User Agent");
                                return ua.GetString();
                            }
                        }
                    }

                    logger.Warn($"Status code received from the API: {response.StatusCode}. Return empty User Agent");
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Unable to fetch dynamic user agent due to: '{e.Message}'");
            }

            return string.Empty;
        }

        internal static async Task<string> GetUserAgent(IHttpClient httpClient, ILogger logger, CancellationToken cancellationToken)
        {
            var ua = await GetUserAgentFromApi(httpClient, logger, cancellationToken);
            if (!string.IsNullOrEmpty(ua))
            {
                return ua;
            }

            logger.Warn("User agent was not received from the API. Choosing from predefined");
            return UserAgents[Random.Next(UserAgents.Count)];
        }

        #endregion

        /// <summary>
        /// &lt;video name&gt; (&lt;year&gt;) (&lt;trailer name&gt;)
        /// </summary>
        /// <param name="intro"></param>
        /// <returns></returns>
        internal static string GetPartialTrailerName(KpTrailer intro)
        {
            var filmName = intro.PremierDate?.Year > 0 ? $"{intro.VideoName} ({intro.PremierDate?.Year})" : intro.VideoName;
            return string.Equals(intro.VideoName, intro.TrailerName) ? filmName : $"{filmName} ({intro.TrailerName})";
        }

        internal static string GetKinopoiskTrailerId(string url)
        {
            var match = KpWidgetPlayer.Match(url);
            if (!match.Success)
            {
                match = S3YandexNet.Match(url);
            }

            return match.Success ? match.Groups["id"].Value : string.Empty;
        }
    }
}
