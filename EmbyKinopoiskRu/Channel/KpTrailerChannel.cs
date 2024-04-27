using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;
using EmbyKinopoiskRu.YtDownloader;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
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
        private static readonly Dictionary<YoutubeType, YoutubeDownloader> YoutubeDownloaderDictionary = new Dictionary<YoutubeType, YoutubeDownloader>();
        private readonly ILogger _log;
        private readonly Random _random = new Random();
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogManager _logManager;
        private readonly IHttpClient _httpClient;

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
        /// <param name="serverConfigurationManager"></param>
        public KpTrailerChannel(
            ILogManager logManager,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            _availableTranslations = EmbyHelper.GetAvailableTransactions("KpTrailerChannel");
            _httpClient = httpClient;
            _logManager = logManager;
        }

        /// <inheritdoc />
        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            return PluginConfiguration.KinopoiskDev.Equals(Plugin.Instance.Configuration.ApiType)
                ? query.FolderId == null
                    ? await GetRootFolders(cancellationToken).ConfigureAwait(false)
                    : await GetStreamItems(query, cancellationToken).ConfigureAwait(false)
                : new ChannelItemResult();
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
            _log.Info("GetRootFolders. Fetch collections from API");
            var toReturn = new ChannelItemResult
            {
                TotalRecordCount = 0
            };

            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                _log.Info($"Intros path from the plugin configuration is invalid: '{basePath}'");
                return toReturn;
            }

            var selectedCollections = Plugin.Instance.Configuration.CollectionsList
                .Where(c => c.IsEnable)
                .Select(c => c.Id)
                .ToList();
            if (selectedCollections.Count == 0)
            {
                _log.Info("No collection was selected");
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

            _log.Info($"Fetched {toReturn.TotalRecordCount} collections");
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
            _log.Info($"GetStreamItems. InternalChannelItemQuery: [FolderId: '{query.FolderId}', Limit: '{query.Limit}', SortBy: '{query.SortBy}'" +
                      $", SortDescending: '{query.SortDescending}', StartIndex: '{query.StartIndex}', UserId: '{query.UserId}']");

            var toReturn = new ChannelItemResult();
            if (cancellationToken.IsCancellationRequested)
            {
                _log.Warn("Cancellation was requested");
                return toReturn;
            }

            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                _log.Error($"Intros path from the plugin configuration is invalid: '{basePath}'");
                return toReturn;
            }

            var introsQuality = Plugin.Instance.Configuration.IntrosQuality;
            if (introsQuality <= 0)
            {
                _log.Error($"Intros quality is invalid: '{introsQuality}'");
                return toReturn;
            }

            _log.Info($"Intros quality from the plugin configuration: '{introsQuality}', collection slug: '{query.FolderId}'");

            if (YoutubeDownloaderDictionary.Count == 0)
            {
                YoutubeDownloaderDictionary.Add(YoutubeType.Y2Mate, new Y2MateDownloader(_logManager, _httpClient));
                YoutubeDownloaderDictionary.Add(YoutubeType.Tomp3, new Tomp3Downloader(_logManager, _httpClient));
            }

            var intros = await Plugin.Instance.GetKinopoiskService().GetTrailersFromCollectionAsync(query.FolderId, cancellationToken);
            _log.Info($"Found {intros.Count} trailers");
            var collectionFolder = Path.Combine(basePath, query.FolderId);
            List<ChannelItemInfo> items = new List<ChannelItemInfo>();
            foreach (var intro in intros)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _log.Info($"Cancellation was requested. Exit with fetched till now intros: '{items.Count}'");
                    break;
                }

                string youtubeId = YtHelper.GetYoutubeId(intro.Url);
                if (youtubeId.Contains("http"))
                {
                    _log.Warn($"The intro is not from youtube - ask for plugin update: '{intro.Url}' and provide the link");
                    continue;
                }

                if (Plugin.Instance.Configuration.OnlyRussianTrailers && !IsContainsRussianCharacter(intro.TrailerName))
                {
                    _log.Info($"Configured to download only trailers on Russian. Skip trailer name: '{intro.TrailerName}', link: '{intro.Url}'");
                    continue;
                }

                var filePath = GetFileFromFileSystem(collectionFolder, youtubeId);
                var partialFileName = YtHelper.GetPartialIntroName(intro);
                if (string.IsNullOrEmpty(filePath))
                {
                    _log.Info($"The file doesn't exist for youtubeId: '{youtubeId}'. Going to download it");
                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(MinDownloadWaitingSeconds, MaxDownloadWaitingSeconds)), cancellationToken);
                    filePath = await YoutubeDownloaderDictionary[YoutubeType.Tomp3].DownloadYoutubeLink(youtubeId, partialFileName, introsQuality, collectionFolder, cancellationToken);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        _log.Info("Failed to download video via tomp3.cc, trying y2mate");
                        filePath = await YoutubeDownloaderDictionary[YoutubeType.Y2Mate].DownloadYoutubeLink(youtubeId, partialFileName, introsQuality, collectionFolder, cancellationToken);
                    }

                    if (string.IsNullOrEmpty(filePath))
                    {
                        _log.Warn($"Unable to download '{intro.Url}'");
                        continue;
                    }
                }

                if (filePath.Contains(YtHelper.NotExistsOnYoutube))
                {
                    _log.Info("Looks like the trailer doesn't exist on youtube anymore. Skip");
                    continue;
                }

                items.Add(new ChannelItemInfo
                {
                    ContentType = ChannelMediaContentType.MovieExtra,
                    OriginalTitle = intro.VideoName,
                    Name = intro.VideoName,
                    Id = youtubeId,
                    ImageUrl = intro.ImageUrl,
                    MediaType = ChannelMediaType.Video,
                    ExtraType = ExtraType.Trailer,
                    PremiereDate = intro.PremierDate,
                    Overview = intro.Overview,
                    Type = ChannelItemType.Media,
                    ProviderIds = intro.ProviderIds,
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
                });
                _log.Info($"Added intro [name: '{intro.VideoName}', youtubeId: '{youtubeId}', file path: '{filePath}']");
            }

            _log.Info($"Prepared for a view {items.Count} trailers");
            return new ChannelItemResult
            {
                Items = items,
                TotalRecordCount = items.Count
            };
        }

        private static string GetFileFromFileSystem(string collectionFolder, string youtubeId)
        {
            if (!Directory.Exists(collectionFolder))
            {
                return string.Empty;
            }

            var files = Directory.GetFiles(collectionFolder, $"*[{youtubeId}].*");
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

        private static bool IsContainsRussianCharacter(string name)
        {
            return string.IsNullOrWhiteSpace(name)
                   || name.Length != ContainsRussianChar.Replace(name, String.Empty).Length;
        }

        #endregion
    }
}
