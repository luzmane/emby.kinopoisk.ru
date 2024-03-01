using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial;
using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    /// <inheritdoc />
    public class CreateKinopoiskIdTask : IScheduledTask, IConfigurableScheduledTask
    {
        private readonly ILogger _log;
        private static bool s_isScanRunning;
        private static readonly object ScanLock = new object();
        private const int CHUNK_SIZE = 150;

        private readonly ILibraryManager _libraryManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations;
        private Plugin Plugin { get; set; }

        /// <inheritdoc />
        public string Name => GetTranslation().Name;

        /// <inheritdoc />
        public string Key => "KinopoiskFromOther";

        /// <inheritdoc />
        public string Description => GetTranslation().Description;

        /// <inheritdoc />
        public string Category => GetTranslation().Category;

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => false;

        /// <inheritdoc />
        public bool IsLogged => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateKinopoiskIdTask"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        public CreateKinopoiskIdTask(
                 ILogManager logManager,
                 ILibraryManager libraryManager,
                 IServerConfigurationManager serverConfigurationManager,
                 IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger(GetType().Name);
            _libraryManager = libraryManager;
            _serverConfigurationManager = serverConfigurationManager;
            _jsonSerializer = jsonSerializer;

            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            Plugin = Plugin.Instance;

            _availableTranslations = EmbyHelper.GetAvailableTransactions($"ScheduledTasks.{Key}");
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        /// <inheritdoc />
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _log.Info("Task started");
            if (s_isScanRunning)
            {
                _log.Info("Another task is running, exiting");
                _log.Info("Task finished");
                return;
            }
            lock (ScanLock)
            {
                if (s_isScanRunning)
                {
                    _log.Info("Another task is running, exiting");
                    _log.Info("Task finished");
                    return;
                }
                s_isScanRunning = true;
            }
            try
            {
                IKinopoiskRuService kinopoiskRuService = Plugin.GetKinopoiskService();
                if (kinopoiskRuService is KinopoiskUnofficialService)
                {
                    _log.Error("KinopoiskUnofficial doesn't support this task. Exit");
                    return;
                }

                _log.Info("Searching for movies/series without KP id, but with IMDB or TMDB");
                QueryResult<BaseItem> itemsToUpdateResult = _libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Movie), nameof(Series) },
                    MissingAnyProviderId = new[] { Plugin.PluginKey },
                    HasAnyProviderId = new[] { MetadataProviders.Imdb.ToString(), MetadataProviders.Tmdb.ToString() },
                });
                _log.Info($"Found {itemsToUpdateResult.TotalRecordCount} items");
                progress.Report(10d);

                if (itemsToUpdateResult.TotalRecordCount == 0)
                {
                    _log.Info("Nothing to update. Exiting");
                }
                else
                {
                    var imdbList = itemsToUpdateResult.Items.Where(m => !string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Imdb.ToString())));
                    await UpdateItemsByProviderIdAsync(imdbList, MetadataProviders.Imdb.ToString(), cancellationToken);
                    _log.Info("Finishing update Kinopoisk IDs by Imdb ID");
                    progress.Report(50d);

                    var tmdbList = itemsToUpdateResult.Items
                        .Where(m => string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Imdb.ToString())))
                        .Where(m => !string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Tmdb.ToString())));
                    await UpdateItemsByProviderIdAsync(tmdbList, MetadataProviders.Tmdb.ToString(), cancellationToken);
                    _log.Info("Finishing update Kinopoisk IDs by Tmdb ID");
                    progress.Report(100d);
                }
            }
            finally
            {
                s_isScanRunning = false;
                _log.Info("Task finished");
            }
        }

        private async Task UpdateItemsByProviderIdAsync(IEnumerable<BaseItem> itemsToUpdate, string providerId, CancellationToken cancellationToken)
        {
            var itemsCount = itemsToUpdate.Count();
            _log.Info($"Requesting KP ID for {itemsCount} items by {providerId} ID from API");
            var count = (int)Math.Ceiling((double)itemsCount / CHUNK_SIZE);
            for (var i = 0; i < count; i++)
            {
                IEnumerable<BaseItem> workingList = itemsToUpdate.Skip(CHUNK_SIZE * i).Take(CHUNK_SIZE);
                ApiResult<Dictionary<string, long>> fetchedIds = await Plugin.GetKinopoiskService()
                    .GetKpIdByAnotherIdAsync(providerId, workingList.Select(m => m.GetProviderId(providerId)), cancellationToken);
                if (fetchedIds.HasError)
                {
                    var processed = i == 0 ? workingList.Count() : (CHUNK_SIZE * (i - 1)) + workingList.Count();
                    _log.Warn($"Unable to fetch data from API. Processed {processed} of {itemsCount}");
                }
                else
                {
                    UpdateProviderId(workingList, providerId, fetchedIds.Item);
                }
            }
        }
        private void UpdateProviderId(IEnumerable<BaseItem> workingList, string providerId, Dictionary<string, long> fetchedIds)
        {
            foreach (BaseItem item in workingList)
            {
                var id = item.GetProviderId(providerId);
                if (fetchedIds.TryGetValue(id, out var itemId))
                {
                    item.SetProviderId(Plugin.PluginKey, itemId.ToString(CultureInfo.InvariantCulture));
                    item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                }
                else
                {
                    _log.Info($"Unable to find Kp id for {item.Name} ({providerId}:{id})");
                }
            }
        }
        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
    }
}
