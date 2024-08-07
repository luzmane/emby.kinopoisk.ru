using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.TrailerDownloader.Youtube.Model;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.TrailerDownloader.Youtube
{
    internal abstract class YoutubeDownloader : ITrailerDownloader
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        protected YoutubeDownloader(ILogManager logManager, IHttpClient httpClient, string loggerName)
        {
            _logger = logManager.GetLogger(loggerName);
            _httpClient = httpClient;
        }

        public async Task<string> DownloadTrailerByLink(string youtubeId, string videoName, int preferableQuality, string basePath, CancellationToken cancellationToken)
        {
            try
            {
                var userAgent = await TrailerDlHelper.GetUserAgent(_httpClient, _logger, _cancellationToken);
                var analyzeResponse = await Analyze(youtubeId, userAgent, _cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download '{youtubeId}'");
                    return string.Empty;
                }

                if (analyzeResponse.Count == 0)
                {
                    if (await IsVideoExistsOnYoutube(youtubeId, _cancellationToken))
                    {
                        _logger.Error($"Unable to analyze Youtube video: {youtubeId}");
                    }
                    else
                    {
                        _logger.Info($"The trailer doesn't exist on youtube anymore: {youtubeId}");
                        return CreateMissedFilePlaceholder(youtubeId, basePath);
                    }

                    return string.Empty;
                }

                KeyValuePair<int, TrailerFormat> trailerFormat;
                foreach (KeyValuePair<int, TrailerFormat> tmp in analyzeResponse.OrderByDescending(k => k.Key))
                {
                    trailerFormat = tmp;
                    if (tmp.Key <= preferableQuality)
                    {
                        break;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download '{youtubeId}'");
                    return string.Empty;
                }

                _logger.Debug($"Chosen link: YoutubeId: '{youtubeId}', Quality: '{trailerFormat.Value.Quality}', Size: '{trailerFormat.Value.Size}', Format: '{trailerFormat.Value.Format}'");
                var downloadLink = await Convert(youtubeId, trailerFormat.Value.Key, userAgent, _cancellationToken);
                if (string.IsNullOrEmpty(downloadLink))
                {
                    _logger.Error($"Unable to convert analyzed video to download link: '{youtubeId}'");
                    return string.Empty;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download '{youtubeId}'");
                    return string.Empty;
                }

                return await DownloadLink(
                    Path.Combine(basePath, TrailerDlHelper.GetIntroName(videoName, youtubeId, trailerFormat.Value.Format)),
                    downloadLink,
                    userAgent,
                    _cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Stop download due to: '{ex.Message}'", ex);
                return string.Empty;
            }
        }

        private string CreateMissedFilePlaceholder(string youtubeId, string basePath)
        {
            if (!VerifyRootFolderExists(basePath))
            {
                return String.Empty;
            }

            try
            {
                var filePath = Path.Combine(basePath, $"[{youtubeId}]{TrailerDlHelper.NotExists}");
                File.WriteAllText(filePath, string.Empty);
                _logger.Debug($"Mock file created: '{filePath}'");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to create mock file: '{youtubeId}{TrailerDlHelper.NotExists}', due to: '{ex.Message}'", ex);
            }

            return string.Empty;
        }

        private async Task<Dictionary<int, TrailerFormat>> Analyze(string youtubeId, string userAgent, CancellationToken cancellationToken)
        {
            Dictionary<int, TrailerFormat> toReturn = new Dictionary<int, TrailerFormat>(0);

            try
            {
                var options = GetAnalyzeRequestOptions(youtubeId, userAgent, cancellationToken);
                using (HttpResponseInfo response = await _httpClient.Post(options))
                {
                    using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken))
                    {
                        var rootElement = jsonDocument.RootElement;
                        if (!rootElement.TryGetProperty("mess", out var mess) || !mess.ValueEquals(string.Empty))
                        {
                            _logger.Warn($"Unable to analyze. Mess: '{mess}'");
                            return toReturn;
                        }

                        if (!rootElement.TryGetProperty("status", out var status) || !"ok".Equals(status.GetString()))
                        {
                            _logger.Warn($"Unable to analyze. Status: '{status}'");
                            return toReturn;
                        }

                        if (rootElement.TryGetProperty("links", out var links))
                        {
                            toReturn = ParseLinksAnalyzeResponse(links);
                        }

                        return toReturn;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException($"Unable to analyze due to: '{e.Message}'", e);
            }

            return toReturn;
        }

        private async Task<string> Convert(string youtubeId, string videoKey, string userAgent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(videoKey))
            {
                _logger.Warn($"Invalid video key: {videoKey}");
                return string.Empty;
            }

            try
            {
                var options = GetConvertRequestOptions(youtubeId, videoKey, userAgent, cancellationToken);
                using (HttpResponseInfo response = await _httpClient.Post(options))
                {
                    using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken))
                    {
                        var rootElement = jsonDocument.RootElement;
                        if (!rootElement.TryGetProperty("mess", out var mess) || !mess.ValueEquals(string.Empty))
                        {
                            _logger.Warn($"Unable to get download links. Mess: '{mess}'");
                            return string.Empty;
                        }

                        if (!rootElement.TryGetProperty("status", out var status) || !"ok".Equals(status.GetString()))
                        {
                            _logger.Warn($"Unable to get download links. Status: '{status}'");
                            return string.Empty;
                        }

                        if (rootElement.TryGetProperty("dlink", out var dlink))
                        {
                            return dlink.GetString();
                        }

                        _logger.Warn($"Download link is not available: {rootElement.GetRawText()}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException($"Some exception occured during the link preparation: {e.Message}", e);
            }

            return string.Empty;
        }

        private async Task<string> DownloadLink(string filePath, string downloadLink, string userAgent, CancellationToken cancellationToken)
        {
            if (File.Exists(filePath))
            {
                _logger.Warn($"File already exists: '{filePath}'");
                return string.Empty;
            }

            string parentPath = Path.GetDirectoryName(filePath);
            if (!VerifyRootFolderExists(parentPath))
            {
                return string.Empty;
            }

            var downloadUrl = new Uri(downloadLink);
            var options = new HttpRequestOptions
            {
                AcceptHeader = "*/*",
                CancellationToken = cancellationToken,
                EnableHttpCompression = false,
                Host = downloadUrl.Host,
                Referer = GetDownloadReferer(),
                Url = downloadLink,
                UserAgent = userAgent,
                Progress = new Progress<double>(),
                TimeoutMs = 300_000,
                LogRequestAsDebug = true,
                DownloadFilePath = filePath
            };
            options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            options.RequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            options.RequestHeaders.Add("Sec-Fetch-Dest", "document");
            options.RequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            options.RequestHeaders.Add("Sec-Fetch-Site", "cross-site");
            options.RequestHeaders.Add("Sec-Fetch-User", "?1");
            try
            {
                filePath = await _httpClient.GetTempFile(options);
                _logger.Debug($"The file is downloaded to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to download the file {filePath} due to '{ex.Message}'", ex);
                filePath = string.Empty;
            }

            return filePath;
        }

        private async Task<bool> IsVideoExistsOnYoutube(string youtubeId, CancellationToken cancellationToken)
        {
            try
            {
                var options = new HttpRequestOptions
                {
                    AcceptHeader = "*/*",
                    CacheMode = CacheMode.Unconditional,
                    CacheLength = TimeSpan.FromHours(12),
                    CancellationToken = cancellationToken,
                    DecompressionMethod = CompressionMethod.Gzip,
                    EnableHttpCompression = true,
                    Url = $"https://www.youtube.com/watch?v={youtubeId}",
                    EnableDefaultUserAgent = true,
                    LogRequestAsDebug = true,
                    Sanitation =
                    {
                        SanitizeDefaultParams = false
                    }
                };
                using (HttpResponseInfo response = await _httpClient.GetResponse(options))
                {
                    using (var streamReader = new StreamReader(response.Content))
                    {
                        var content = await streamReader.ReadToEndAsync();
                        return !content.Contains("This video isn't available anymore")
                               && !content.Contains("Video unavailable");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException($"Unable to verify if the video still exists on youtube. Youtube ID: '{youtubeId}', error: '{e.Message}'", e);
            }

            return false;
        }

        protected abstract HttpRequestOptions GetAnalyzeRequestOptions(string youtubeId, string userAgent, CancellationToken cancellationToken);

        protected abstract Dictionary<int, TrailerFormat> ParseLinksAnalyzeResponse(JsonElement linksElement);

        protected abstract HttpRequestOptions GetConvertRequestOptions(string youtubeId, string videoKey, string userAgent, CancellationToken cancellationToken);

        protected abstract string GetDownloadReferer();

        private bool VerifyRootFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                _logger.Error($"Provided path in invalid: '{folderPath}'");
                return false;
            }

            if (Directory.Exists(folderPath))
            {
                return true;
            }

            try
            {
                _ = Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to create collection folder '{folderPath}' due to '{ex.Message}'", ex);
                return false;
            }

            return true;
        }
    }
}
