using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;
using EmbyKinopoiskRu.TrailerDownloader.Youtube;
using EmbyKinopoiskRu.TrailerDownloader;
using EmbyKinopoiskRu.TrailerDownloader.Kinopoisk;

using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Channel
{
    /// <summary>
    /// Kinopoisk Trailers channel
    /// </summary>
    public class KpTrailerChannel : IChannel // IHasChangeEvent, IHasChannelFeatures, IRequiresMediaInfoCallback
    {
        private const int MaxDownloadWaitingSeconds = 15;
        private const int MinDownloadWaitingSeconds = 5;
        private static readonly Regex ContainsRussianChar = new Regex("[а-яА-Я]+", RegexOptions.Compiled);
        private static readonly Dictionary<DownloaderType, ITrailerDownloader> TrailersDownloaderDictionary = new Dictionary<DownloaderType, ITrailerDownloader>();
        private static readonly CancellationToken CancellationTokenNone = CancellationToken.None;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogManager _logManager;
        private readonly IHttpClient _httpClient;
        private readonly IApplicationPaths _appPaths;
        private readonly IFfmpegManager _ffmpegManager;
        private readonly IMediaProbeManager _mediaProbeManager;
        private static readonly object Lock = new object();

        /// <inheritdoc />
        public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

        /// <inheritdoc />
        public string Name => GetTranslation().Name;

        /// <inheritdoc />
        public string Description => GetTranslation().Description;

        /// <summary>
        /// Kinopoisk Trailers channel constructor
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="httpClient"></param>
        /// <param name="jsonSerializer"></param>
        /// <param name="ffmpegManager"></param>
        /// <param name="mediaProbeManager"></param>
        /// <param name="serverConfigurationManager"></param>
        /// <param name="appPaths"></param>
        public KpTrailerChannel(
            ILogManager logManager,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            IApplicationPaths appPaths,
            IFfmpegManager ffmpegManager,
            IMediaProbeManager mediaProbeManager,
            IServerConfigurationManager serverConfigurationManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            _availableTranslations = EmbyHelper.GetAvailableTranslations(nameof(KpTrailerChannel));
            _httpClient = httpClient;
            _logManager = logManager;
            _ffmpegManager = ffmpegManager;
            _appPaths = appPaths;
            _mediaProbeManager = mediaProbeManager;
        }

        /// <inheritdoc />
        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            return query.FolderId == null
                ? await GetRootFolders(cancellationToken).ConfigureAwait(false)
                : await GetStreamItems(query, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            return type != ImageType.Primary && type != ImageType.Thumb
                ? throw new ArgumentException($"Unsupported image type: {type}")
                : Task.FromResult(new DynamicImageResponse
                {
                    Format = ImageFormat.Png,
                    Stream = this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".thumb.png")
                });
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Primary
            };
        }

        #region private

        /// <summary>
        /// Build collections folder structure, where each folder is a collection
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ChannelItemResult> GetRootFolders(CancellationToken cancellationToken)
        {
            _logger.Info("GetRootFolders. Fetch collections from API");
            var toReturn = new ChannelItemResult
            {
                TotalRecordCount = 0
            };

            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                _logger.Info($"Intros path from the plugin configuration is invalid: '{basePath}'");
                return toReturn;
            }

            var selectedCollections = Plugin.Instance.Configuration.CollectionsList
                .Where(c => c.IsEnable)
                .Select(c => c.Id)
                .ToList();
            if (selectedCollections.Count == 0)
            {
                _logger.Info("No collection was selected");
                return toReturn;
            }

            var collections = (await Plugin.Instance.GetKinopoiskService().GetKpCollectionsAsync(cancellationToken))
                .Where(c => selectedCollections.Contains(c.Slug));

            toReturn.Items = collections.Select(collection => new ChannelItemInfo
                {
                    FolderType = ChannelFolderType.Container,
                    MediaType = ChannelMediaType.Video,
                    Name = collection.Name,
                    Id = collection.Slug,
                    Type = ChannelItemType.Folder,
                    ImageUrl = collection.Cover?.PreviewUrl ?? collection.Cover?.Url
                })
                .ToList();
            toReturn.TotalRecordCount = toReturn.Items.Count;

            _logger.Info($"Fetched {toReturn.TotalRecordCount} collections");
            return toReturn;
        }

        /// <summary>
        /// Fill collection with videos, download if needed
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ChannelItemResult> GetStreamItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.Info($"GetStreamItems. InternalChannelItemQuery: [FolderId: '{query.FolderId}', Limit: '{query.Limit}', SortBy: '{query.SortBy}'" +
                         $", SortDescending: '{query.SortDescending}', StartIndex: '{query.StartIndex}', UserId: '{query.UserId}']");

            var toReturn = new ChannelItemResult();
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info("Cancellation was requested");
                return toReturn;
            }

            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                _logger.Error($"Intros path from the plugin configuration is invalid: '{basePath}'");
                return toReturn;
            }

            var introsQuality = Plugin.Instance.Configuration.IntrosQuality;
            if (introsQuality <= 0)
            {
                _logger.Error($"Intros quality is invalid: '{introsQuality}'");
                return toReturn;
            }

            _logger.Info($"Intros quality from the plugin configuration: '{introsQuality}', collection slug: '{query.FolderId}'");

            lock (Lock)
            {
                if (TrailersDownloaderDictionary.Count == 0)
                {
                    lock (Lock)
                    {
                        TrailersDownloaderDictionary.Add(DownloaderType.Y2Mate, new Y2MateDownloader(_logManager, _httpClient));
                        TrailersDownloaderDictionary.Add(DownloaderType.Tomp3, new Tomp3Downloader(_logManager, _httpClient));
                        TrailersDownloaderDictionary.Add(DownloaderType.EmbyKp, new KinopoiskDownloader(_logManager, _httpClient, _appPaths, _ffmpegManager));
                    }
                }
            }

            var trailers = await Plugin.Instance.GetKinopoiskService().GetTrailersFromCollectionAsync(query.FolderId, cancellationToken);
            _logger.Info($"Found {trailers.Count} trailers");
            var collectionFolder = Path.Combine(basePath, query.FolderId);
            List<ChannelItemInfo> items = new List<ChannelItemInfo>();
            var maxTrailerDuration = Plugin.Instance.Configuration.TrailerMaxDuration * Constants.OneMinuteInSec;
            _logger.Info($"Max trailer duration: {maxTrailerDuration}");
            foreach (var trailer in trailers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation was requested. Exit with fetched '{items.Count}' trailers till now");
                    break;
                }

                if (!IsValidTrailer(trailer))
                {
                    _logger.Warn($"This video not a trailer: '{trailer}'");
                    continue;
                }

                if (Plugin.Instance.Configuration.OnlyRussianTrailers
                    && !IsRussianTrailer(trailer))
                {
                    _logger.Info($"Configured to download only trailers on Russian. Skip trailer name: '{trailer.TrailerName}', link: '{trailer.Url}'");
                    continue;
                }

                ChannelItemInfo item = null;
                if (trailer.Url.Contains("kinopoisk")
                    || trailer.Url.Contains("trailers.s3.mds.yandex.net"))
                {
                    item = await ChannelItemInfoFromKinopoisk(trailer, collectionFolder, cancellationToken);
                }
                else if (trailer.Url.Contains("youtu"))
                {
                    item = await ChannelItemInfoFromYoutube(trailer, collectionFolder, cancellationToken);
                }
                else
                {
                    _logger.Warn($"Such link is not supported, ask for plugin update and provide the link: '{trailer.Url}'");
                }

                if (item != null
                    && await VerifyTrailerDuration(item, maxTrailerDuration, cancellationToken))
                {
                    _logger.Debug($"Added channel item [name: '{item.Name}', Id: '{item.Id}']");
                    items.Add(item);
                }
                else if (item != null)
                {
                    _logger.Info($"Skip adding channel item [name: '{item.Name}', Id: '{item.Id}'] - it doesn't suits by duration criteria");
                    RemoveFile(item.MediaSources[0].Path);
                }
            }

            _logger.Info($"Prepared for a view {items.Count} trailers");
            return new ChannelItemResult
            {
                Items = items,
                TotalRecordCount = items.Count
            };
        }

        private void RemoveFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to delete extra long trailer '{path}' due to: '{ex.Message}'", ex);
            }
        }

        private async Task<bool> VerifyTrailerDuration(ChannelItemInfo item, long maxTrailerDuration, CancellationToken cancellationToken)
        {
            if (maxTrailerDuration == 0)
            {
                _logger.Debug("Trailer duration limit turned off - max trailer duration is 0");
                return true;
            }

            long runTimeTicks = 0;
            try
            {
                var mediaInfoRequest = new MediaInfoRequest()
                {
                    MediaType = DlnaProfileType.Video,
                    ExtractChapters = false,
                    MediaSource = new MediaSourceInfo()
                    {
                        Protocol = MediaProtocol.File,
                        Path = item.MediaSources[0].Path,
                        Type = MediaSourceType.Default,
                        IsRemote = false
                    }
                };

                var mediaInfo = await _mediaProbeManager.GetMediaInfo(mediaInfoRequest, cancellationToken);
                runTimeTicks = mediaInfo.RunTimeTicks ?? 0;
                var durationSec = runTimeTicks / 10_000_000;
                if (durationSec == 0)
                {
                    _logger.Warn($"Unable to detect trailer duration, ignore verification result. RunTimeTicks: '{runTimeTicks}', Path: '{item.MediaSources[0].Path}'");
                    return true;
                }

                _logger.Debug($"Max trailer duration '{maxTrailerDuration}', media duration '{durationSec}' sec");
                return TrailerDlHelper.CheckTrailerDuration(durationSec, maxTrailerDuration);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to detect trailer duration due to: '{ex.Message}'. RunTimeTicks: '{runTimeTicks}'", ex);
                return true;
            }
        }

        private async Task<ChannelItemInfo> ChannelItemInfoFromYoutube(KpTrailer trailer, string collectionFolder, CancellationToken cancellationToken)
        {
            string youtubeId = TrailerDlHelper.GetYoutubeId(trailer.Url);
            if (youtubeId.Contains("http"))
            {
                _logger.Warn($"Such youtube link is not supported, ask for plugin update and provide the link: '{trailer.Url}'");
                return null;
            }

            var filePath = GetFileFromFileSystem(collectionFolder, youtubeId);
            var partialFileName = TrailerDlHelper.GetPartialTrailerName(trailer);
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.Info($"The file doesn't exist for youtubeId: '{youtubeId}' ('{trailer.TrailerName}' from '{trailer.VideoName}'). Going to download it");
                await Task.Delay(TimeSpan.FromSeconds(_random.Next(MinDownloadWaitingSeconds, MaxDownloadWaitingSeconds)), CancellationTokenNone);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download youtube Id: '{youtubeId}'");
                    return null;
                }

                ITrailerDownloader downloader;
                lock (Lock)
                {
                    downloader = TrailersDownloaderDictionary[DownloaderType.Tomp3];
                }

                filePath = await downloader.DownloadTrailerByLink(youtubeId, partialFileName, Plugin.Instance.Configuration.IntrosQuality, collectionFolder, cancellationToken);
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.Info("Failed to download video via tomp3.cc, trying y2mate");
                    lock (Lock)
                    {
                        downloader = TrailersDownloaderDictionary[DownloaderType.Y2Mate];
                    }

                    filePath = await downloader.DownloadTrailerByLink(youtubeId, partialFileName, Plugin.Instance.Configuration.IntrosQuality, collectionFolder, cancellationToken);
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.Error($"Unable to download '{trailer.Url}'");
                    return null;
                }
            }

            if (filePath.Contains(TrailerDlHelper.NotExists))
            {
                _logger.Info("Looks like the trailer doesn't exist on youtube anymore. Skip");
                return null;
            }

            _logger.Info($"Downloaded trailer [name: '{trailer.VideoName}', youtubeId: '{youtubeId}', file path: '{filePath}']");
            return new ChannelItemInfo
            {
                ContentType = ChannelMediaContentType.MovieExtra,
                OriginalTitle = trailer.VideoName,
                Name = trailer.VideoName,
                Id = youtubeId,
                ImageUrl = trailer.ImageUrl,
                MediaType = ChannelMediaType.Video,
                ExtraType = ExtraType.Trailer,
                PremiereDate = trailer.PremierDate,
                Overview = trailer.Overview,
                Type = ChannelItemType.Media,
                ProviderIds = trailer.ProviderIds,
                MediaSources = new List<MediaSourceInfo>
                {
                    new MediaInfo
                    {
                        Protocol = MediaProtocol.File,
                        Id = youtubeId,
                        Name = $"{partialFileName} {youtubeId}",
                        Path = filePath,
                        Type = MediaSourceType.Default,
                        IsRemote = false
                    }
                }
            };
        }

        private async Task<ChannelItemInfo> ChannelItemInfoFromKinopoisk(KpTrailer trailer, string collectionFolder, CancellationToken cancellationToken)
        {
            string kpId = TrailerDlHelper.GetKinopoiskTrailerId(trailer.Url);
            if (string.IsNullOrWhiteSpace(kpId))
            {
                _logger.Warn($"Such kinopoisk link is not supported, ask for plugin update and provide the link: '{trailer.Url}'");
                return null;
            }

            var kinopoiskId = $"{KinopoiskDownloader.KpFileSuffix}{kpId}";
            var filePath = GetFileFromFileSystem(collectionFolder, kinopoiskId);
            var partialFileName = TrailerDlHelper.GetPartialTrailerName(trailer);
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.Info($"The file doesn't exist for kinopoiskId: '{kpId}' ('{trailer.TrailerName}' from '{trailer.VideoName}'). Going to download it");
                await Task.Delay(TimeSpan.FromSeconds(_random.Next(MinDownloadWaitingSeconds, MaxDownloadWaitingSeconds)), CancellationTokenNone);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info($"Cancellation requested. Stop download KpID: '{kinopoiskId}'");
                    return null;
                }

                ITrailerDownloader downloader;
                lock (Lock)
                {
                    downloader = TrailersDownloaderDictionary[DownloaderType.EmbyKp];
                }

                filePath = await downloader.DownloadTrailerByLink(kpId, partialFileName, Plugin.Instance.Configuration.IntrosQuality, collectionFolder, cancellationToken);
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.Error($"Unable to download '{trailer.Url}'");
                    return null;
                }
            }

            if (filePath.Contains(TrailerDlHelper.NotExists))
            {
                _logger.Info("Looks like the trailer doesn't exist on Kinopoisk anymore. Skip");
                return null;
            }

            _logger.Info($"Downloaded trailer [name: '{trailer.VideoName}', kinopoiskId: '{kinopoiskId}', file path: '{filePath}']");
            return new ChannelItemInfo
            {
                ContentType = ChannelMediaContentType.MovieExtra,
                OriginalTitle = trailer.VideoName,
                Name = trailer.VideoName,
                Id = kinopoiskId,
                ImageUrl = trailer.ImageUrl,
                MediaType = ChannelMediaType.Video,
                ExtraType = ExtraType.Trailer,
                PremiereDate = trailer.PremierDate,
                Overview = trailer.Overview,
                Type = ChannelItemType.Media,
                ProviderIds = trailer.ProviderIds,
                MediaSources = new List<MediaSourceInfo>
                {
                    new MediaInfo
                    {
                        Protocol = MediaProtocol.File,
                        Id = kinopoiskId,
                        Name = $"{partialFileName} {kinopoiskId}",
                        Path = filePath,
                        Type = MediaSourceType.Default,
                        IsRemote = false
                    }
                }
            };
        }

        private static string GetFileFromFileSystem(string collectionFolder, string trailerId)
        {
            if (!Directory.Exists(collectionFolder))
            {
                return string.Empty;
            }

            var files = Directory.GetFiles(collectionFolder, $"*[{trailerId}].*");
            if (files.Length == 1)
            {
                return files[0];
            }

            foreach (var file in files)
            {
                File.Delete(file);
            }

            return string.Empty;
        }

        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }

        private static bool IsValidTrailer(KpTrailer trailer)
        {
            var videoName = trailer.VideoName;
            if (string.IsNullOrWhiteSpace(videoName))
            {
                // no video name - invalid
                return false;
            }

            var trailerName = trailer.TrailerName;
            if (string.IsNullOrWhiteSpace(trailerName))
            {
                // no trailer name - invalid
                return false;
            }

            videoName = videoName.ToLower();
            trailerName = trailerName.ToLower();
            var name = trailerName.Replace(videoName, String.Empty);
            if (string.IsNullOrWhiteSpace(name))
            {
                // trailer name equal to video name (trailer doesn't have its own name)
                return true;
            }

            var hasStopWord = TrailerDlHelper.TrailerStopWordList.Any(n => name.Contains(n));
            if (hasStopWord)
            {
                return false;
            }

            return true;
        }

        private static bool IsRussianTrailer(KpTrailer trailer)
        {
            var name = trailer.TrailerName.ToLower().Replace(trailer.VideoName.ToLower(), String.Empty);
            if (string.IsNullOrWhiteSpace(name))
            {
                // trailer name equal to video name (trailer doesn't have its own name)
                return false;
            }

            var hasRussianChars = name.Length != ContainsRussianChar.Replace(name, string.Empty).Length;
            if (!hasRussianChars)
            {
                return false;
            }

            var hasStopWord = TrailerDlHelper.RussianStopWordList.Any(n => name.Contains(n));
            if (hasStopWord)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
