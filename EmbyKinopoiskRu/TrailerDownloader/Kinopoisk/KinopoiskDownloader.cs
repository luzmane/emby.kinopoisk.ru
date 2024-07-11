using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.TrailerDownloader.M3UParser;
using EmbyKinopoiskRu.TrailerDownloader.M3UParser.Model;

using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.TrailerDownloader.Kinopoisk
{
    internal class KinopoiskDownloader : ITrailerDownloader
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IApplicationPaths _appPaths;
        private readonly IFfmpegManager _ffmpegManager;
        private readonly M3U8Parser _m3U8Parser;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        private const string LinkTemplate = "https://widgets.kinopoisk.ru/discovery/trailer/{0}?onlyPlayer=1&autoplay=1&cover=1";
        private const string FinalExtension = "mp4";
        private const string MergeAudioVideoTemplate = "-i \"{0}\" -i \"{1}\" -c copy \"{2}\"";
        private const string MergeFilesTemplate = "-f concat -safe 0 -i \"{0}\" -c copy \"{1}\"";

        public const string KpFileSuffix = "kpt-";

        private static readonly Regex FindMasterPlaylistOnPage = new Regex(@".+data-state[^>]*>(?<json>[^<]+)<\/script>.*",
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal KinopoiskDownloader(ILogManager logManager, IHttpClient httpClient, IApplicationPaths appPaths, IFfmpegManager ffmpegManager)
        {
            _logger = logManager.GetLogger(nameof(KinopoiskDownloader));
            _httpClient = httpClient;
            _appPaths = appPaths;
            _ffmpegManager = ffmpegManager;
            _m3U8Parser = new M3U8Parser(logManager);
        }

        public async Task<string> DownloadTrailerByLink(string kinopoiskId, string videoName, int preferableQuality, string baseOutputFolder, CancellationToken cancellationToken)
        {
            var userAgent = await TrailerDlHelper.GetUserAgent(_httpClient, _logger, _cancellationToken);
            var pageContent = await DownloadPlayerPage(kinopoiskId, userAgent);
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info($"Cancellation requested. Stop download KpID: '{kinopoiskId}'");
                return string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(pageContent))
            {
                _logger.Error($"Player page content is empty. KpID: '{kinopoiskId}'");
                return string.Empty;
            }

            var masterM3U8Url = GetMasterPlaylist(pageContent, kinopoiskId);
            if (string.IsNullOrWhiteSpace(masterM3U8Url))
            {
                _logger.Error("Master playlist url was not found");
                return string.Empty;
            }

            var masterM3U8Playlist = await DownloadM3U8(masterM3U8Url, userAgent, kinopoiskId);
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info($"Cancellation requested. Stop download KpID: '{kinopoiskId}'");
                return string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(masterM3U8Playlist))
            {
                _logger.Error($"Failed to download master playlist. KpID: '{kinopoiskId}'");
                return string.Empty;
            }

            _logger.Debug($"Parse master playlist for KpID: '{kinopoiskId}'");
            var chosenStreams = _m3U8Parser.ParseMasterM3U8(masterM3U8Url, masterM3U8Playlist, preferableQuality);
            var tempFolder = GetTempFilesBaseFolder();
            try
            {
                _logger.Debug($"Prepare video part for KpID: '{kinopoiskId}'");
                var videoFile = await PrepareFilePart(chosenStreams.xStreamInfs, userAgent, kinopoiskId, tempFolder, cancellationToken);

                _logger.Debug($"Prepare audio part for KpID: '{kinopoiskId}'");
                var audioFile = await PrepareFilePart(chosenStreams.xMedia, userAgent, kinopoiskId, tempFolder, cancellationToken);

                return await MergeVideoAudio(tempFolder, videoFile, audioFile, baseOutputFolder, kinopoiskId, videoName);
            }
            finally
            {
                RemoveTempFolder(tempFolder);
            }
        }

        private async Task<string> PrepareFilePart(IEnumerable<IHasUrl> iHasUrls, string userAgent, string kinopoiskId, string tempFolder, CancellationToken cancellationToken)
        {
            foreach (var iHasUrl in iHasUrls)
            {
                var m3U8Content = await DownloadM3U8(iHasUrl.Url, userAgent, kinopoiskId);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download KpID: '{kinopoiskId}'");
                    return string.Empty;
                }
                else if (string.IsNullOrWhiteSpace(m3U8Content))
                {
                    _logger.Info($"Failed to download playlist for KpID: '{kinopoiskId}', trying another link");
                    continue;
                }

                _logger.Debug($"Parse playlist for KpID: '{kinopoiskId}'");
                var links = _m3U8Parser.ParseM3U8(m3U8Content);
                List<string> files = await DownloadFiles(tempFolder, links, userAgent, kinopoiskId);
                if (!files.Any())
                {
                    _logger.Info($"Failed to download files for KpID: '{kinopoiskId}', trying another link");
                    continue;
                }

                _logger.Debug($"Merging parts for KpID: '{kinopoiskId}'. File count: {files.Count}");
                var file = await MergeFiles(tempFolder, files, kinopoiskId);
                if (string.IsNullOrEmpty(file))
                {
                    _logger.Info($"Failed to merge file for KpID: '{kinopoiskId}', trying another link");
                    continue;
                }

                return file;
            }

            return String.Empty;
        }

        private async Task<string> MergeVideoAudio(string tempFolder, string videoFile, string audioFile, string baseOutputFolder, string kinopoiskId, string videoName)
        {
            var output = Path.Combine(baseOutputFolder, TrailerDlHelper.GetIntroName(videoName, $"{KpFileSuffix}{kinopoiskId}", FinalExtension));
            _logger.Debug($"Merge audio and video into '{output}'");
            if (!VerifyRootFolderExists(baseOutputFolder))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(audioFile))
            {
                _logger.Debug($"Audio part doesn't exist, using video part as output for KpID: '{kinopoiskId}'");
                try
                {
                    File.Move(videoFile, output);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"Failed to move video part for KpID: '{kinopoiskId}' due to: '{ex.Message}'", ex);
                }
            }
            else
            {
                var ffmpegLogFileName = $"{kinopoiskId}_{Guid.NewGuid()}_ffmpeg.log";
                try
                {
                    using (var runner = _ffmpegManager.CreateFfMpegRunner(Plugin.PluginKey, Path.Combine(tempFolder, ffmpegLogFileName)))
                    {
                        runner.LogOutput(LogSeverity.Warn);
                        runner.Start(string.Format(MergeAudioVideoTemplate, audioFile, videoFile, output));
                        await runner.WaitForExitAsync(360_000, _cancellationToken);
                        await Task.Delay(TimeSpan.FromMilliseconds(100), _cancellationToken);
                        if (runner.LastExitCode != 0)
                        {
                            var ffmpegFinalLog = Path.Combine(_appPaths.LogDirectoryPath, ffmpegLogFileName);
                            _logger.Error($"Unable to merge audio and video parts by ffmpeg for KinopoiskId: '{kinopoiskId}'. Process return: '{runner.LastExitCode}'. FFMPEG logs: '{ffmpegFinalLog}'");
                            BackupFfmpegLogs(tempFolder, ffmpegLogFileName, ffmpegFinalLog);
                            output = String.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var ffmpegFinalLog = Path.Combine(_appPaths.LogDirectoryPath, ffmpegLogFileName);
                    _logger.ErrorException($"Unable to merge audio and video parts by ffmpeg for KinopoiskId: '{kinopoiskId}': {ex.Message}. FFMPEG logs: '{ffmpegFinalLog}'", ex);
                    BackupFfmpegLogs(tempFolder, ffmpegLogFileName, ffmpegFinalLog);
                    return string.Empty;
                }
            }

            return output;
        }

        private void BackupFfmpegLogs(string srcFolder, string logFileName, string dst)
        {
            try
            {
                File.Move(Path.Combine(srcFolder, logFileName), dst);
            }
            catch (Exception ex2)
            {
                _logger.ErrorException($"Unable to move ffmpeg log file to log folder due to: {ex2.Message}", ex2);
            }
        }

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

        private async Task<string> MergeFiles(string outputFolder, List<string> files, string kinopoiskId)
        {
            var baseFolder = Directory.GetParent(files[0]);
            if (baseFolder == null || !baseFolder.Exists)
            {
                _logger.Error($"Unable to get parent folder of downloaded files: '{string.Join(", ", files)}'");
                return string.Empty;
            }

            var guid = Guid.NewGuid();
            var output = Path.Combine(outputFolder, guid + ".ts");
            var input = Path.Combine(baseFolder.FullName, guid + ".txt");
            File.WriteAllLines(input, files.Select(l => $"file '{l}'"));
            var ffmpegLogFileName = $"{kinopoiskId}_{guid}_ffmpeg.log";
            try
            {
                using (var runner = _ffmpegManager.CreateFfMpegRunner(Plugin.PluginKey, Path.Combine(baseFolder.FullName, ffmpegLogFileName)))
                {
                    runner.LogOutput(LogSeverity.Warn);
                    runner.Start(string.Format(MergeFilesTemplate, input, output));
                    await runner.WaitForExitAsync(360_000, _cancellationToken);
                    if (runner.LastExitCode != 0)
                    {
                        var ffmpegFinalLog = Path.Combine(_appPaths.LogDirectoryPath, ffmpegLogFileName);
                        _logger.Error($"Unable to merge files by ffmpeg for KinopoiskId: '{kinopoiskId}'. Process return: '{runner.LastExitCode}'. FFMPEG logs: '{ffmpegFinalLog}'");
                        BackupFfmpegLogs(baseFolder.FullName, ffmpegLogFileName, ffmpegFinalLog);
                        output = String.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                var ffmpegFinalLog = Path.Combine(_appPaths.LogDirectoryPath, ffmpegLogFileName);
                _logger.ErrorException($"Unable to merge file parts by ffmpeg for KinopoiskId: '{kinopoiskId}': {ex.Message}. FFMPEG logs: '{ffmpegFinalLog}'", ex);
                BackupFfmpegLogs(baseFolder.FullName, ffmpegLogFileName, ffmpegFinalLog);
                return string.Empty;
            }

            return output;
        }

        private async Task<List<string>> DownloadFiles(string baseFolder, List<string> links, string userAgent, string kinopoiskId)
        {
            _logger.Debug($"Download file from playlist for KpID: '{kinopoiskId}'");
            List<string> toReturn = new List<string>();
            int cnt = 0;
            foreach (var link in links)
            {
                try
                {
                    var uri = new Uri(link);
                    var downloadFile = Path.Combine(baseFolder, $"{cnt++:0000000}_{uri.Segments[uri.Segments.Length - 1]}");
                    _logger.Debug($"Download '{uri.AbsoluteUri}' to '{downloadFile}'");
                    var options = new HttpRequestOptions
                    {
                        AcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
                        CacheMode = CacheMode.None,
                        CancellationToken = _cancellationToken,
                        EnableHttpCompression = false,
                        Host = uri.Host,
                        UserAgent = userAgent,
                        TimeoutMs = 120_000,
                        Url = link,
                        EnableKeepAlive = true,
                        EnableDefaultUserAgent = false,
                        Progress = new Progress<double>(),
                        Sanitation =
                        {
                            SanitizeDefaultParams = false
                        },
                        DownloadFilePath = downloadFile,
                        LogRequestAsDebug = true
                    };
                    options.RequestHeaders.Add("Priority", "u=1");
                    options.RequestHeaders.Add("Sec-Fetch-Dest", "document");
                    options.RequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    options.RequestHeaders.Add("Sec-Fetch-Site", "none");
                    options.RequestHeaders.Add("Sec-Fetch-User", "?1");
                    options.RequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                    options.RequestHeaders.Add("charset", "UTF-8");
                    options.RequestHeaders.Add("TE", "trailers");

                    toReturn.Add(await _httpClient.GetTempFile(options));
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"Failed to download part of trailer due to: '{ex.Message}'", ex);
                    toReturn.Clear();
                    break;
                }
            }

            return toReturn;
        }

        private void RemoveTempFolder(string folder)
        {
            try
            {
                _logger.Debug($"Delete temp folder: '{folder}'");
                Directory.Delete(folder, true);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Failed to delete temp folder: '{ex.Message}'. Please delete it manually: '{folder}'", ex);
            }
        }

        private string GetTempFilesBaseFolder()
        {
            var baseFolder = Path.Combine(this._appPaths.TempDirectory, Guid.NewGuid().ToString());
            while (Directory.Exists(baseFolder))
            {
                baseFolder = Path.Combine(this._appPaths.TempDirectory, Guid.NewGuid().ToString());
            }

            try
            {
                _ = Directory.CreateDirectory(baseFolder);
                return baseFolder;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to create temp folder for temp files due to: '{ex.Message}'", ex);
                return string.Empty;
            }
        }

        private async Task<string> DownloadM3U8(string url, string userAgent, string kinopoiskId)
        {
            _logger.Debug($"Download playlist for KpID: '{kinopoiskId}'");
            try
            {
                var uri = new Uri(url);
                var options = new HttpRequestOptions
                {
                    AcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
                    CacheMode = CacheMode.None,
                    CancellationToken = _cancellationToken,
                    DecompressionMethod = CompressionMethod.Gzip,
                    EnableHttpCompression = true,
                    Host = uri.Host,
                    UserAgent = userAgent,
                    TimeoutMs = 120_000,
                    Url = url,
                    EnableKeepAlive = true,
                    EnableDefaultUserAgent = false,
                    LogRequestAsDebug = true,
                    Sanitation =
                    {
                        SanitizeDefaultParams = false
                    }
                };
                options.RequestHeaders.Add("Priority", "u=1");
                options.RequestHeaders.Add("Sec-Fetch-Dest", "document");
                options.RequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                options.RequestHeaders.Add("Sec-Fetch-Site", "none");
                options.RequestHeaders.Add("Sec-Fetch-User", "?1");
                options.RequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                options.RequestHeaders.Add("charset", "UTF-8");
                options.RequestHeaders.Add("TE", "trailers");

                using (HttpResponseInfo response = await _httpClient.GetResponse(options))
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            using (var reader = new StreamReader(response.Content))
                            {
                                return await reader.ReadToEndAsync();
                            }
                        case HttpStatusCode.Found:
                            var location = response.Headers["Location"];
                            _logger.Info($"Item was moved to: '{location}'");
                            return await DownloadM3U8($"https://{uri.Host}/{location}", userAgent, kinopoiskId);
                        default:
                            _logger.Error($"Received HTTP code: '{response.StatusCode}'. Skip download");
                            return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Failed to download page by url '{url}' due to: '{ex.Message}'", ex);
                return string.Empty;
            }
        }

        private string GetMasterPlaylist(string pageContent, string kinopoiskId)
        {
            _logger.Debug($"Parsing page to get master playlist url for KpID: '{kinopoiskId}'");
            Match match = FindMasterPlaylistOnPage.Match(pageContent);
            if (!match.Success)
            {
                _logger.Error($"Unable to find master playlist data on the page. KinopoiskId: '{kinopoiskId}'");
                return string.Empty;
            }

            var json = HttpUtility.UrlDecode(match.Groups["json"].Value);
            try
            {
                using (var document = JsonDocument.Parse(json))
                {
                    var root = document.RootElement;
                    var trailerM3U8MasterUrl = root
                        .GetProperty("models")
                        .GetProperty("trailers")
                        .GetProperty(kinopoiskId)
                        .GetProperty("streamUrl")
                        .GetString();
                    _logger.Debug($"Master playlist url is: '{trailerM3U8MasterUrl}'");
                    return trailerM3U8MasterUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to get master playlist url from given json: '{json}'", ex);
                return string.Empty;
            }
        }

        private async Task<string> DownloadPlayerPage(string kinopoiskId, string userAgent)
        {
            _logger.Debug($"Download player page for KpID: '{kinopoiskId}'");
            var url = string.Format(LinkTemplate, kinopoiskId);
            try
            {
                var options = new HttpRequestOptions
                {
                    AcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
                    CacheMode = CacheMode.None,
                    CancellationToken = _cancellationToken,
                    DecompressionMethod = CompressionMethod.Gzip,
                    EnableHttpCompression = true,
                    Host = new Uri(url).Host,
                    UserAgent = userAgent,
                    TimeoutMs = 120_000,
                    Url = url,
                    EnableKeepAlive = true,
                    LogRequestAsDebug = true,
                    EnableDefaultUserAgent = false,
                    Sanitation =
                    {
                        SanitizeDefaultParams = false
                    }
                };
                options.RequestHeaders.Add("Priority", "u=1");
                options.RequestHeaders.Add("Sec-Fetch-Dest", "document");
                options.RequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                options.RequestHeaders.Add("Sec-Fetch-Site", "none");
                options.RequestHeaders.Add("Sec-Fetch-User", "?1");
                options.RequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                options.RequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                options.RequestHeaders.Add("charset", "UTF-8");

                using (HttpResponseInfo response = await _httpClient.GetResponse(options))
                {
                    using (var reader = new StreamReader(response.Content))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Failed to download page by url '{url}' due to: '{ex.Message}'. KpID: '{kinopoiskId}'", ex);
                return string.Empty;
            }
        }
    }
}
